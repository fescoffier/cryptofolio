using Cryptofolio.Infrastructure.Caching;
using Cryptofolio.Infrastructure.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<CurrencyTickersUpsertedEventHandler> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="CurrencyTickersUpsertedEventHandler"/>.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        public CurrencyTickersUpsertedEventHandler(CurrencyTickerCache cache, ILogger<CurrencyTickersUpsertedEventHandler> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(CurrencyTickersUpsertedEvent @event, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Handling the event occurence {0} of type {1}.", @event.Id, typeof(CurrencyTickersUpsertedEvent).FullName);
            _logger.LogDebug("Tickers tickers in cache.");
            await _cache.StoreTickersAsync(@event.Tickers.Select(t => new Ticker
            {
                Pair = new(t.Currency.Code, t.VsCurrency.Code),
                Timestamp = t.Timestamp,
                Value = t.Value
            }));
            _logger.LogDebug("Tickers stored in cache.");
            // TODO: Trigger wallet balance recompute.
            return Unit.Value;
        }
    }
}
