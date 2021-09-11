using System;
using System.Text.Json.Serialization;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Models an currency ticker.
    /// </summary>
    public class CurrencyTicker
    {
        /// <summary>
        /// The currency that owns this ticker.
        /// </summary>
        public Currency Currency { get; set; }

        /// <summary>
        /// The currency which the price is calculated against.
        /// </summary>
        [JsonPropertyName("vs_currency")]
        public Currency VsCurrency { get; set; }

        /// <summary>
        /// The timestamp.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// The value expressed in <see cref="VsCurrency"/>.
        /// </summary>
        public decimal Value { get; set; }
    }
}
