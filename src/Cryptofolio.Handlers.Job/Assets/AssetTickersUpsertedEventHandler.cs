using Cryptofolio.Infrastructure.Caching;
using Cryptofolio.Infrastructure.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<AssetTickersUpsertedEventHandler> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="AssetTickersUpsertedEventHandler"/>.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        public AssetTickersUpsertedEventHandler(CurrencyTickerCache cache, ILogger<AssetTickersUpsertedEventHandler> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(AssetTickersUpsertedEvent @event, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Handling the event occurence {0} of type {1}.", @event.Id, typeof(AssetTickersUpsertedEvent).FullName);
            _logger.LogDebug("Tickers tickers in cache.");
            await _cache.StoreTickersAsync(@event.Tickers.Select(t => new Ticker
            {
                Pair = new(t.Asset.Symbol, t.VsCurrency.Code),
                Timestamp = t.Timestamp,
                Value = t.Value
            }));
            _logger.LogDebug("Tickers stored in cache.");
            // TODO: Trigger wallet balance recompute.
            return Unit.Value;
        }
    }
}
