using System;
using System.Collections.Generic;

namespace Cryptofolio.Core.Entities
{
    /// <summary>
    /// Models an inserted or updated event occured on <see cref="Entities.AssetTicker"/>.
    /// </summary>
    public class AssetTickerUpsertedEvent : IEvent
    {
        /// <inheritdoc/>
        public DateTimeOffset Date { get; init; }

        /// <inheritdoc/>
        public string UserId { get; init; }

        /// <inheritdoc/>
        public string Username { get; init; }

        /// <inheritdoc/>
        public string Category => EventConstants.Categories.Asset;

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
