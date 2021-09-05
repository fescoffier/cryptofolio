using System.Text.Json.Serialization;

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
        /// The quantity held.
        /// </summary>
        public decimal Qty { get; set; }

        /// <summary>
        /// The initial value in the wallet currency.
        /// It's the sum of all initial values of every transaction in the holding.
        /// </summary>
        [JsonPropertyName("initial_value")]
        public decimal InitialValue { get; set; }

        /// <summary>
        /// The current value in the wallet currency.
        /// </summary>
        [JsonPropertyName("current_value")]
        public decimal CurrentValue { get; set; }

        /// <summary>
        /// The change in percent.
        /// </summary>
        public decimal Change { get; set; }
    }
}
