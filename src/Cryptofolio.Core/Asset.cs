namespace Cryptofolio.Core
{
    /// <summary>
    /// Models an asset.
    /// </summary>
    public class Asset
    {
        /// <summary>
        /// The id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The symbol.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// The name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description.
        /// </summary>
        public string Description { get; set; }
    }
}
