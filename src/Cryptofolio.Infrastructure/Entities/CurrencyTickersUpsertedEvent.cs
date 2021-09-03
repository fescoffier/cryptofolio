using System;
using System.Collections.Generic;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Models an inserted or updated event occured on a list of <see cref="CurrencyTicker"/>.
    /// </summary>
    public class CurrencyTickersUpsertedEvent : IEvent
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
        /// The tickers.
        /// </summary>
        public IEnumerable<CurrencyTicker> Tickers { get; set; }
    }
}
