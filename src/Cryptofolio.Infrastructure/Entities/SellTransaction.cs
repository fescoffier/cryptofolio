namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Models a transaction of type "Sell".
    /// </summary>
    public class SellTransaction : Transaction
    {
        /// <summary>
        /// The exchange on which it was sold.
        /// </summary>
        public Exchange Exchange { get; set; }

        /// <summary>
        /// The currency it was sold for.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// The prise per asset in <see cref="Currency"/>.
        /// </summary>
        public decimal Price { get; set; }
    }
}
