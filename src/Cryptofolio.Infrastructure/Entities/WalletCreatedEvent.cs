using System;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Models a created event occured on <see cref="Entities.Wallet"/>.
    /// </summary>
    public class WalletCreatedEvent : IEvent
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
        /// The created wallet.
        /// </summary>
        public Wallet Wallet { get; init; }
    }
}
