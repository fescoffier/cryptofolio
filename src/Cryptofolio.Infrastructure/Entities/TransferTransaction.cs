namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Models a transaction of type "Transfer".
    /// </summary>
    public class TransferTransaction : Transaction
    {
        /// <summary>
        /// The source.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The destination.
        /// </summary>
        public string Destination { get; set; }
    }
}
