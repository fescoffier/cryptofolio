using StackExchange.Redis;

namespace Cryptofolio.Infrastructure.Caching
{
    /// <summary>
    /// Provides a cache for <see cref="Entities.AssetTicker"/>.
    /// </summary>
    public class AssetTickerCache : TickerCache
    {
        /// <inheritdoc/>
        protected override string HashKey => "AssetTickers";

        /// <inheritdoc/>
        public AssetTickerCache(IDatabase database) : base(database)
        {
        }
    }
}
