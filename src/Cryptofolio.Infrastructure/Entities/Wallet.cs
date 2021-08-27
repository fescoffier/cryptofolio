using System.Collections.Generic;

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
