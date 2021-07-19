using System;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Models an inserted or updated event occured on <see cref="Entities.Exchange"/>.
    /// </summary>
    public class ExchangeInfosUpsertedEvent : IEvent
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
        /// The exchange that has been inserted/updated.
        /// </summary>
        public Exchange Exchange { get; init; }
    }
}
