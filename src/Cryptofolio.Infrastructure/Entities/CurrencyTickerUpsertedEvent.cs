using System;
using System.Collections.Generic;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Models an inserted or updated event occured on <see cref="CurrencyTicker"/>.
    /// </summary>
    public class CurrencyTickerUpsertedEvent : IEvent
    {
        /// <inheritdoc/>
        public string Id { get; init; }

        /// <inheritdoc/>
        public DateTimeOffset Date { get; init; }

        /// <inheritdoc/>
        public string UserId { get; init; }

        /// <inheritdoc/>
        public string Category => InfrastructureConstants.Events.Categories.Currency;

        /// <summary>
        /// The currency.
        /// </summary>
        public Currency Currency { get; init; }

        /// <summary>
        /// The versus currency.
        /// </summary>
        public IEnumerable<Currency> VsCurrencies { get; init; }
    }
}
