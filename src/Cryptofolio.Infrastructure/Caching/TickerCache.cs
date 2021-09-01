using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cryptofolio.Infrastructure.Caching
{
    /// <summary>
    /// Provides a Redis cache for <see cref="Ticker"/> of any pairable entity.
    /// </summary>
    public abstract class TickerCache
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            Converters =
            {
                new TickerPairJsonConverter()
            }
        };

        private readonly IDatabase _database;

        /// <summary>
        /// The Redis hash key.
        /// </summary>
        protected abstract string HashKey { get; }

        /// <summary>
        /// Creates a new instance of <see cref="TickerCache"/>.
        /// </summary>
        /// <param name="database">The Redis database.</param>
        protected TickerCache(IDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// Stores the provided tickers in Redis.
        /// </summary>
        /// <param name="tickers">The tickers.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous task.</returns>
        public Task StoreTickersAsync(IEnumerable<Ticker> tickers) =>
            _database.HashSetAsync(HashKey, tickers.Select(t => new HashEntry(t.Pair.ToString(), JsonSerializer.Serialize(t, SerializerOptions))).ToArray());

        /// <summary>
        /// Returns the tickers of the specified pairs.
        /// </summary>
        /// <param name="pairs">The pairs.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous task an contains the tickers.</returns>
        public async Task<IEnumerable<Ticker>> GetTickersAsync(params TickerPair[] pairs)
        {
            var values = await _database.HashGetAsync(HashKey, pairs.Select(p => new RedisValue(p.ToString())).ToArray());
            return values.Where(v => v.HasValue).Select(v => JsonSerializer.Deserialize<Ticker>(v, SerializerOptions)).ToArray();
        }
    }
}
