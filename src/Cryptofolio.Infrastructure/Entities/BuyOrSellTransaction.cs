using System.Text.Json.Serialization;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Models a transaction of type "Buy" or "Sell".
    /// </summary>
    public class BuyOrSellTransaction : Transaction
    {
        /// <summary>
        /// Defines the transaction type:
        /// - Buy
        /// - Sell
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The currency it was bought with/sold for.
        /// </summary>
        public Currency Currency { get; set; }

        /// <summary>
        /// The price per asset in the currency.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The value of the transaction in the currency.
        /// </summary>
        [JsonPropertyName("initial_value")]
        public decimal InitialValue { get; set; }

        /// <summary>
        /// The change in percent.
        /// </summary>
        public decimal Change { get; set; }
    }
}
