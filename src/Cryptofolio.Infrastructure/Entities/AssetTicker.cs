using System;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Models an asset's ticker.
    /// </summary>
    public class AssetTicker
    {
        /// <summary>
        /// The asset that owns this ticker.
        /// </summary>
        public Asset Asset { get; set; }

        /// <summary>
        /// The timestamp.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// The value expressed in <see cref="VsCurrency"/>.
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// The currency price of the ticker.
        /// </summary>
        public string VsCurrency { get; set; }
    }
}
