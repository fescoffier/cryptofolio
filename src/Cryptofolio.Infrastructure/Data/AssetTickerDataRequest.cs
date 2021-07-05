using System.Collections.Generic;

namespace Cryptofolio.Infrastructure.Data
{
    /// <summary>
    /// Models a data request for <see cref="Core.Entities.AssetTicker"/>.
    /// </summary>
    public class AssetTickerDataRequest : DataRequest
    {
        /// <summary>
        /// The assets ids.
        /// </summary>
        public IEnumerable<string> Ids { get; set; }

        /// <summary>
        /// The versus currencies.
        /// </summary>
        public IEnumerable<string> VsCurrencies { get; set; }
    }
}
