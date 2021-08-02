using System;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Models an updated event occured on <see cref="Entities.Wallet"/>.
    /// </summary>
    public class WalletUpdatedEvent : IEvent
    {
        /// <inheritdoc/>
        public string Id { get; init; }

        /// <inheritdoc/>
        public DateTimeOffset Date { get; init; }

        /// <inheritdoc/>
        public string UserId { get; init; }

        /// <inheritdoc/>
        public string Category => InfrastructureConstants.Events.Categories.Wallet;

        /// <summary>
        /// The updated wallet.
        /// </summary>
        public Wallet Wallet { get; init; }
    }
}
