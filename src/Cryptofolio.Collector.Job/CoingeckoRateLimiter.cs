using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Collector.Job
{
    public class CoingeckoRateLimiter
    {
        private TimeSpan _rateLimiterKeyExpiration;
        private TimeSpan _rateLimiterCheckInterval;

        private readonly IDatabase _database;
        private readonly ISystemClock _systemClock;
        private readonly IOptionsMonitor<CoingeckoOptions> _optionsMonitor;
        private readonly ILogger<CoingeckoRateLimiter> _logger;

        private CoingeckoOptions Options => _optionsMonitor.CurrentValue;

        public CoingeckoRateLimiter(
            IDatabase database,
            ISystemClock systemClock,
            IOptionsMonitor<CoingeckoOptions> optionsMonitor,
            ILogger<CoingeckoRateLimiter> logger)
        {
            _database = database;
            _systemClock = systemClock;
            _optionsMonitor = optionsMonitor;
            _logger = logger;

            _optionsMonitor.OnChange((options, _) =>
            {
                _rateLimiterKeyExpiration = TimeSpan.FromSeconds(options.RateLimiterKeyExiprationSeconds);
                _rateLimiterCheckInterval = TimeSpan.FromSeconds(options.RateLimiterCheckIntervalMs);
            });
            _rateLimiterKeyExpiration = TimeSpan.FromSeconds(Options.RateLimiterKeyExiprationSeconds);
            _rateLimiterCheckInterval = TimeSpan.FromSeconds(Options.RateLimiterCheckIntervalMs);
        }

        public async Task CallAsync(CancellationToken cancellationToken)
        {
            var count = await _database.StringIncrementAsync(Options.RateLimiterKey);
            if (count == 1)
            {
                await _database.KeyExpireAsync(Options.RateLimiterKey, _rateLimiterKeyExpiration);
            }
            else if (count >= Options.RateLimiterMaxValue)
            {
                while (await _database.KeyExistsAsync(Options.RateLimiterKey))
                {
                    await Task.Delay(_rateLimiterCheckInterval, cancellationToken);
                }
                await _database.KeyExpireAsync(Options.RateLimiterKey, _rateLimiterKeyExpiration);
            }
        }
    }
}
