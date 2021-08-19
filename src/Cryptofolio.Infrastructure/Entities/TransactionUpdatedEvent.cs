using System;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Models an updated event occured on <see cref="Entities.Transaction"/>.
    /// </summary>
    public class TransactionUpdatedEvent : IEvent
    {
        /// <inheritdoc/>
        public string Id { get; init; }

        /// <inheritdoc/>
        public DateTimeOffset Date { get; init; }

        /// <inheritdoc/>
        public string UserId { get; init; }

        /// <inheritdoc/>
        public string Category => InfrastructureConstants.Events.Categories.Transaction;

        /// <summary>
        /// The updated transaction.
        /// </summary>
        public Transaction Transaction { get; set; }
    }
}
