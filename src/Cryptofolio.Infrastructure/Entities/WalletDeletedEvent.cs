using System;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Models a deleted event occured on <see cref="Entities.Wallet"/>.
    /// </summary>
    public class WalletDeletedEvent : IEvent
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
        /// The deleted wallet.
        /// </summary>
        public Wallet Wallet { get; init; }
    }
}
