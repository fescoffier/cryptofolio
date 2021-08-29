using System;
using System.Collections.Generic;

namespace Cryptofolio.Collector.Job.Fixer
{
    /// <summary>
    /// Models the latest rates of a currency.
    /// </summary>
    public class FixerLatestRatesResponse : FixerResponseBase
    {
        /// <summary>
        /// The timestamp.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// The date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The base currency.
        /// </summary>
        public string Base { get; set; }

        /// <summary>
        /// The vs currencies rates.
        /// </summary>
        public Dictionary<string, decimal> Rates { get; set; }
    }
}
