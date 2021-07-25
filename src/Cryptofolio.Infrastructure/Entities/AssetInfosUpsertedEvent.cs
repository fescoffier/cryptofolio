using System;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Models an inserted or updated event occured on <see cref="Entities.Asset"/>.
    /// </summary>
    public class AssetInfosUpsertedEvent : IEvent
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
        /// The asset that has been inserted/updated.
        /// </summary>
        public Asset Asset { get; init; }
    }
}
