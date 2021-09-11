using System;
using System.Text.Json.Serialization;

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
        /// The initial value (computed with the ticker at the transaction date) of the transaction.
        /// </summary>
        [JsonPropertyName("initial_value")]
        public decimal InitialValue { get; set; }

        /// <summary>
        /// The current value (computed with the latest ticker) of the transaction.
        /// </summary>
        [JsonPropertyName("current_value")]
        public decimal CurrentValue { get; set; }

        /// <summary>
        /// The change in percent.
        /// </summary>
        public decimal Change { get; set; }

        /// <summary>
        /// The optional note.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// The type discriminator.
        /// </summary>
        [JsonPropertyName("type_discriminator")]
        public string TypeDiscriminator => GetType().Name;
    }
}
