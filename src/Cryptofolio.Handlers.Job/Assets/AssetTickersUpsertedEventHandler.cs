using Confluent.Kafka;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Balances;
using Cryptofolio.Infrastructure.Caching;
using Cryptofolio.Infrastructure.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Handlers.Job.Assets
{
    /// <summary>
    /// Provides an implementation of <see cref="IPipelineBehavior{IEvent, Unit}"/> to handle events of types <see cref="AssetTickersUpsertedEvent"/>.
    /// It caches the latest assets tickers.
    /// </summary>
    public class AssetTickersUpsertedEventHandler : IPipelineBehavior<AssetTickersUpsertedEvent, Unit>
    {
        private readonly CurrencyTickerCache _cache;
        private readonly KafkaProducerWrapper<string, BulkComputeWalletBalanceRequest> _producerWrapper;
        private readonly ILogger<AssetTickersUpsertedEventHandler> _logger;

        private IProducer<string, BulkComputeWalletBalanceRequest> Producer => _producerWrapper.Producer;

        private KafkaProducerOptions<BulkComputeWalletBalanceRequest> ProducerOptions => _producerWrapper.Options;

        /// <summary>
        /// Creates a new instance of <see cref="AssetTickersUpsertedEventHandler"/>.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="producerWrapper">The producer wrapper.</param>
        /// <param name="logger">The logger.</param>
        public AssetTickersUpsertedEventHandler(
            CurrencyTickerCache cache,
            KafkaProducerWrapper<string, BulkComputeWalletBalanceRequest> producerWrapper,
            ILogger<AssetTickersUpsertedEventHandler> logger)
        {
            _cache = cache;
            _producerWrapper = producerWrapper;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(AssetTickersUpsertedEvent @event, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Handling the event occurence {0} of type {1}.", @event.Id, typeof(AssetTickersUpsertedEvent).FullName);
            _logger.LogDebug("Tickers tickers in cache.");
            await _cache.StoreTickersAsync(@event.Tickers
                .Select(t => new Ticker
                {
                    Pair = new(t.Asset.Symbol, t.VsCurrency.Code),
                    Timestamp = t.Timestamp,
                    Value = t.Value
                })
                .ToArray());
            _logger.LogDebug("Tickers stored in cache.");
            _logger.LogInformation("Triggering bulk wallets balance computing.");
            await Producer.ProduceAsync(
                ProducerOptions.Topic,
                new()
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = new()
                    {
                        AssetIds = @event.Tickers.Select(t => t.Asset.Id).Distinct().ToArray(),
                        CurrencyIds = @event.Tickers.Select(t => t.VsCurrency.Id).Distinct().ToArray()
                    }
                }
            );
            return Unit.Value;
        }
    }
}
