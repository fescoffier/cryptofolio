namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Models a transaction of type "Buy".
    /// </summary>
    public class BuyTransaction : Transaction
    {
        /// <summary>
        /// The exchange on which it was bought.
        /// </summary>
        public Exchange Exchange { get; set; }

        /// <summary>
        /// The currency it was bought with.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// The price per asset in the currency.
        /// </summary>
        public decimal Price { get; set; }
    }
}
