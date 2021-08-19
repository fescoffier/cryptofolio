using System;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Models a transaction.
    /// </summary>
    public abstract class Transaction
    {
        /// <summary>
        /// The id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The date when it occured.
        /// </summary>
        public DateTimeOffset Date { get; set; }

        /// <summary>
        /// The wallet it's attached to.
        /// </summary>
        public Wallet Wallet { get; set; }

        /// <summary>
        /// The asset it concerns.
        /// </summary>
        public Asset Asset { get; set; }

        /// <summary>
        /// The exchange on which it was bought/sold/transfered.
        /// </summary>
        public Exchange Exchange { get; set; }

        /// <summary>
        /// The asset quantity.
        /// </summary>
        public decimal Qty { get; set; }

        /// <summary>
        /// The optional note.
        /// </summary>
        public string Note { get; set; }
    }
}
