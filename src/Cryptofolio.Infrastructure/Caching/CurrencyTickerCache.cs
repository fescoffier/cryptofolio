using StackExchange.Redis;

namespace Cryptofolio.Infrastructure.Caching
{
    /// <summary>
    /// Provides a cache for <see cref="Entities.CurrencyTicker"/>.
    /// </summary>
    public class CurrencyTickerCache : TickerCache
    {
        /// <inheritdoc/>
        protected override string HashKey => "CurrencyTickers";

        /// <inheritdoc/>
        public CurrencyTickerCache(IDatabase database) : base(database)
        {
        }
    }
}
