using System;

namespace Cryptofolio.Core.Entities
{
    /// <summary>
    /// Models an inserted or updated event occured on <see cref="Entities.Exchange"/>.
    /// </summary>
    public class ExchangeInfosUpsertedEvent : IEvent
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
        /// The exchange that has been inserted/updated.
        /// </summary>
        public Exchange Exchange { get; init; }
    }
}
