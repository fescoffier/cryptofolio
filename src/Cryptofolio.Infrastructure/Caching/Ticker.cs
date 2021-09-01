using System;

namespace Cryptofolio.Infrastructure.Caching
{
    /// <summary>
    /// Models a ticker of pair.
    /// </summary>
    public class Ticker
    {
        /// <summary>
        /// The pair.
        /// </summary>
        public TickerPair Pair { get; set; }

        /// <summary>
        /// The timestamp of the value.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// The value.
        /// </summary>
        public decimal Value { get; set; }
    }
}
