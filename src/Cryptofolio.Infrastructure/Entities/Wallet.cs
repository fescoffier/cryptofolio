namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Models a wallet.
    /// </summary>
    public class Wallet
    {
        /// <summary>
        /// The id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The user id that owns the wallet.
        /// </summary>
        public string UserId { get; set; }
    }
}
