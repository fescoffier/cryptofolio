using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cryptofolio.Infrastructure.Data
{
    /// <summary>
    /// Models a data request for <see cref="Entities.AssetTicker"/>.
    /// </summary>
    public class AssetTickerDataRequest : DataRequest
    {
        /// <summary>
        /// The assets ids.
        /// </summary>
        [JsonPropertyName("ids")]
        public IEnumerable<string> Ids { get; set; }

        /// <summary>
        /// The versus currencies.
        /// </summary>
        [JsonPropertyName("vs_currencies")]
        public IEnumerable<string> VsCurrencies { get; set; }
    }
}
