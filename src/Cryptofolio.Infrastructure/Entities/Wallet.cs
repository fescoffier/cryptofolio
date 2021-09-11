using System.Collections.Generic;
using System.Text.Json.Serialization;

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
        /// The currency.
        /// </summary>
        public Currency Currency { get; set; }

        /// <summary>
        /// The initial value in the wallet currency.
        /// It's the sum of all initial values of every transaction in the wallet.
        /// </summary>
        [JsonPropertyName("initial_value")]
        public decimal InitialValue { get; set; }

        /// <summary>
        /// The current value in the currency.
        /// </summary>
        [JsonPropertyName("current_value")]
        public decimal CurrentValue { get; set; }

        /// <summary>
        /// The change in percent.
        /// </summary>
        public decimal Change { get; set; }

        /// <summary>
        /// Defines if it's user selected wallet.
        /// </summary>
        public bool Selected { get; set; }

        /// <summary>
        /// The user id that owns the wallet.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The assets holdings.
        /// </summary>
        public ICollection<Holding> Holdings { get; set; } = new HashSet<Holding>();
    }
}
