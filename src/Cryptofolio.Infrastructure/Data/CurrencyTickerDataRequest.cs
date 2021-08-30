using System.Collections.Generic;

namespace Cryptofolio.Infrastructure.Data
{
    /// <summary>
    /// Models a data requst for <see cref="Entities.CurrencyTicker"/>.
    /// </summary>
    public class CurrencyTickerDataRequest : DataRequest
    {
        /// <summary>
        /// The base currency.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// The versus currencies.
        /// </summary>
        public IEnumerable<string> VsCurrencies { get; set; }
    }
}
