namespace Cryptofolio.Collector.Job
{
    /// <summary>
    /// Configuration options for the Coingecko clients.
    /// </summary>
    public class CoingeckoOptions
    {
        /// <summary>
        /// The URI where the API can be reached.
        /// </summary>
        public string ApiUri { get; set; }

        /// <summary>
        /// The rate limiter key.
        /// </summary>
        public string RateLimiterKey { get; set; }

        /// <summary>
        /// The rate limiter key expiration in seconds.
        /// </summary>
        public int RateLimiterKeyExiprationSeconds { get; set; }

        /// <summary>
        /// The rate limiter max value.
        /// </summary>
        public int RateLimiterMaxValue { get; set; }

        /// <summary>
        /// The rate limiter check interval.
        /// </summary>
        public int RateLimiterCheckIntervalMs { get; set; }
    }
}
