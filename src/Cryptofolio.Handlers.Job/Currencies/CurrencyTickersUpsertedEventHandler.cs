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

namespace Cryptofolio.Handlers.Job.Currencies
{
    /// <summary>
    /// Provides an implementation of <see cref="IPipelineBehavior{IEvent, Unit}"/> to handle events of types <see cref="CurrencyTickersUpsertedEvent"/>.
    /// It caches the latest tickers of a currency.
    /// </summary>
    public class CurrencyTickersUpsertedEventHandler : IPipelineBehavior<CurrencyTickersUpsertedEvent, Unit>
    {
        private readonly CurrencyTickerCache _cache;
        private readonly KafkaProducerWrapper<string, BulkComputeWalletBalanceRequest> _producerWrapper;
        private readonly ILogger<CurrencyTickersUpsertedEventHandler> _logger;

        private IProducer<string, BulkComputeWalletBalanceRequest> Producer => _producerWrapper.Producer;

        private KafkaProducerOptions<BulkComputeWalletBalanceRequest> ProducerOptions => _producerWrapper.Options;

        /// <summary>
        /// Creates a new instance of <see cref="CurrencyTickersUpsertedEventHandler"/>.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="producerWrapper">The producer wrapper.</param>
        /// <param name="logger">The logger.</param>
        public CurrencyTickersUpsertedEventHandler(
            CurrencyTickerCache cache,
            KafkaProducerWrapper<string, BulkComputeWalletBalanceRequest> producerWrapper,
            ILogger<CurrencyTickersUpsertedEventHandler> logger)
        {
            _cache = cache;
            _producerWrapper = producerWrapper;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(CurrencyTickersUpsertedEvent @event, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Handling the event occurence {0} of type {1}.", @event.Id, typeof(CurrencyTickersUpsertedEvent).FullName);
            _logger.LogDebug("Tickers tickers in cache.");
            await _cache.StoreTickersAsync(@event.Tickers
                .Select(t => new Ticker
                {
                    Pair = new(t.Currency.Code, t.VsCurrency.Code),
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
                        CurrencyIds = @event.Tickers.Select(t => t.Currency.Id).Union(@event.Tickers.Select(t => t.VsCurrency.Id)).Distinct().ToArray()
                    }
                }
            );
            return Unit.Value;
        }
    }
}
