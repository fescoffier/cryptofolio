using System;
using System.Collections.Generic;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Models an inserted or updated event occured on <see cref="Entities.AssetTicker"/>.
    /// </summary>
    public class AssetTickerUpsertedEvent : IEvent
    {
        /// <inheritdoc/>
        public string Id { get; init; }

        /// <inheritdoc/>
        public DateTimeOffset Date { get; init; }

        /// <inheritdoc/>
        public string UserId { get; init; }

        /// <inheritdoc/>
        public string Username { get; init; }

        /// <inheritdoc/>
        public string Category => InfrastructureConstants.Events.Categories.Asset;

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
