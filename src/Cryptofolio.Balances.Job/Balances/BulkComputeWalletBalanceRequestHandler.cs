using Confluent.Kafka;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Balances;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Balances.Job.Balances
{
    /// <summary>
    /// Provides an implementation of <see cref="IRequestHandler{BulkComputeWalletBalanceRequest}"/>.
    /// It queues <see cref="ComputeWalletBalanceRequest"/> in batch.
    /// </summary>
    public class BulkComputeWalletBalanceRequestHandler : IRequestHandler<BulkComputeWalletBalanceRequest>
    {
        private readonly CryptofolioContext _context;
        private readonly KafkaProducerWrapper<string, ComputeWalletBalanceRequest> _producerWrapper;
        private readonly IOptionsMonitor<BulkComputeWalletBalanceOptions> _optionsMonitor;
        private readonly ILogger<BulkComputeWalletBalanceRequestHandler> _logger;

        private IProducer<string, ComputeWalletBalanceRequest> Producer => _producerWrapper.Producer;

        private KafkaOptions<ComputeWalletBalanceRequest> ProducerOptions => _producerWrapper.Options;

        private BulkComputeWalletBalanceOptions Options => _optionsMonitor.CurrentValue;

        /// <summary>
        /// Creates a new instance of <see cref="BulkComputeWalletBalanceRequestHandler"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="producerWrapper">The producer wrapper.</param>
        /// <param name="optionsMonitor">The options monitor.</param>
        /// <param name="logger">The logger.</param>
        public BulkComputeWalletBalanceRequestHandler(
            CryptofolioContext context,
            KafkaProducerWrapper<string, ComputeWalletBalanceRequest> producerWrapper,
            IOptionsMonitor<BulkComputeWalletBalanceOptions> optionsMonitor,
            ILogger<BulkComputeWalletBalanceRequestHandler> logger)
        {
            _context = context;
            _producerWrapper = producerWrapper;
            _optionsMonitor = optionsMonitor;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(BulkComputeWalletBalanceRequest request, CancellationToken cancellationToken)
        {
            request.AssetIds ??= Enumerable.Empty<string>();
            request.CurrencyIds ??= Enumerable.Empty<string>();

            _logger.LogInformation("Queueing compute wallet balance requests in bulk.");
            _logger.LogTraceObject("Concerned assets:", request.AssetIds);
            _logger.LogTraceObject("Concerned currencies:", request.CurrencyIds);

            var queryStr = $@"
                select w.*
                from ""data"".""wallet"" w
                where id in (
                    select wallet_id
                    from ""data"".""transaction"" t
                    where t.asset_id in ('{string.Join("','", request.AssetIds)}') or t.currency_id in ('{string.Join("','", request.CurrencyIds)}')
                    union
                    select id from ""data"".""wallet"" where currency_id in ('{string.Join("','", request.CurrencyIds)}')
                )
            ";
            var query = _context.Wallets.FromSqlRaw(queryStr).Select(w => w.Id);
            var queued = 0;
            List<string> items;
            do
            {
                items = await query.Skip(queued).Take(_optionsMonitor.CurrentValue.BatchSize).ToListAsync(cancellationToken);
                items.ForEach(i => Producer.Produce(ProducerOptions.Topic, new() { Key = Guid.NewGuid().ToString(), Value = new() { WalletId = i } }));
                queued += items.Count;
                _logger.LogDebug("Queued {0} request.", items.Count);
            } while (items.Count == _optionsMonitor.CurrentValue.BatchSize);

            Producer.Flush(cancellationToken);

            _logger.LogInformation("Queued {0} compute wallet balance requests.", queued);

            return Unit.Value;
        }
    }
}
