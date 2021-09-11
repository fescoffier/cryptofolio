using System;
using System.Collections.Generic;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Models an inserted or updated event occured on <see cref="AssetTicker"/>.
    /// </summary>
    public class AssetTickersUpsertedEvent : IEvent
    {
        /// <inheritdoc/>
        public string Id { get; init; }

        /// <inheritdoc/>
        public DateTimeOffset Date { get; init; }

        /// <inheritdoc/>
        public string UserId { get; init; }

        /// <inheritdoc/>
        public string Category => InfrastructureConstants.Events.Categories.Asset;

        /// <summary>
        /// The tickers.
        /// </summary>
        public IEnumerable<AssetTicker> Tickers { get; set; }
    }
}
