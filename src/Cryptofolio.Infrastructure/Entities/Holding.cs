namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Models an asset holding in a specific wallet.
    /// </summary>
    public class Holding
    {
        /// <summary>
        /// The id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The wallet that holds the asset.
        /// </summary>
        public Wallet Wallet { get; set; }

        /// <summary>
        /// The asset held.
        /// </summary>
        public Asset Asset { get; set; }

        /// <summary>
        /// The amount held
        /// </summary>
        public decimal Amount { get; set; }
    }
}
