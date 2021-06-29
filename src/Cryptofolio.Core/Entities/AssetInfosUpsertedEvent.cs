using System;

namespace Cryptofolio.Core.Entities
{
    /// <summary>
    /// Models an insert or updated event occured on <see cref="Asset"/>.
    /// </summary>
    public class AssetInfosUpsertedEvent : IEvent
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
        /// The asset that has been inserted/updated.
        /// </summary>
        public Asset Asset { get; init; }
    }
}
