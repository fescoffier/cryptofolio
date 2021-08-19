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
        public string Currency { get; set; }

        /// <summary>
        /// The price per asset in the currency.
        /// </summary>
        public decimal Price { get; set; }
    }
}
