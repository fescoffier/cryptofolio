using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Collector.Job
{
    /// <summary>
    /// Provides an HTTP client handler that can limit the call rate to the Coingecko API.
    /// </summary>
    public class CoingeckoHttpClientHandler : HttpClientHandler
    {
        private TimeSpan _rateLimiterKeyExpiration;
        private TimeSpan _rateLimiterCheckInterval;

        private readonly IDatabase _database;
        private readonly IOptionsMonitor<CoingeckoOptions> _optionsMonitor;
        private readonly ILogger<CoingeckoHttpClientHandler> _logger;

        private CoingeckoOptions Options => _optionsMonitor.CurrentValue;

        /// <summary>
        /// Creates a new instance of <see cref="CoingeckoHttpClientHandler"/>.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="optionsMonitor">The options monitor.</param>
        /// <param name="logger">The logger.</param>
        public CoingeckoHttpClientHandler(
            IDatabase database,
            IOptionsMonitor<CoingeckoOptions> optionsMonitor,
            ILogger<CoingeckoHttpClientHandler> logger)
        {
            _database = database;
            _optionsMonitor = optionsMonitor;
            _logger = logger;

            _optionsMonitor.OnChange((options, _) =>
            {
                _rateLimiterKeyExpiration = TimeSpan.FromSeconds(options.RateLimiterKeyExiprationSeconds);
                _rateLimiterCheckInterval = TimeSpan.FromSeconds(options.RateLimiterCheckIntervalMs);
            });
            _rateLimiterKeyExpiration = TimeSpan.FromSeconds(Options.RateLimiterKeyExiprationSeconds);
            _rateLimiterCheckInterval = TimeSpan.FromMilliseconds(Options.RateLimiterCheckIntervalMs);
        }

        /// <inheritdoc/>
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Making a Coingecko API call.");

            _logger.LogDebug("Incrementing the counter.");
        check: var count = _database.StringIncrement(Options.RateLimiterKey);
            if (count == 1)
            {
                _logger.LogDebug("The counter was just created, setting the expiration.");
                _database.KeyExpire(Options.RateLimiterKey, _rateLimiterKeyExpiration);
            }
            else if (count >= Options.RateLimiterMaxValue)
            {
                _logger.LogDebug("The counter has reached the max value.");
                while ((int)_database.StringGet(Options.RateLimiterKey) >= Options.RateLimiterMaxValue && !cancellationToken.IsCancellationRequested)
                {
                    Task.Delay(_rateLimiterCheckInterval, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                _logger.LogDebug("The counter has been reinitialised.");
                goto check;
            }

            return base.Send(request, cancellationToken);
        }

        /// <inheritdoc/>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Making a Coingecko API call.");

            _logger.LogDebug("Incrementing the counter.");
        check: var count = await _database.StringIncrementAsync(Options.RateLimiterKey);
            if (count == 1)
            {
                _logger.LogDebug("The counter was just created, setting the expiration.");
                await _database.KeyExpireAsync(Options.RateLimiterKey, _rateLimiterKeyExpiration);
            }
            else if (count >= Options.RateLimiterMaxValue)
            {
                _logger.LogDebug("The counter has reached the max value.");
                while ((int)await _database.StringGetAsync(Options.RateLimiterKey) >= Options.RateLimiterMaxValue && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(_rateLimiterCheckInterval, cancellationToken);
                }
                _logger.LogDebug("The counter has been reinitialised.");
                goto check;
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
