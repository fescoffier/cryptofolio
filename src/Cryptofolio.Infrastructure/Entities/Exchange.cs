namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Models an exchange.
    /// </summary>
    public class Exchange
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
        /// The year the it was established.
        /// </summary>
        public long? YearEstablished { get; set; }

        /// <summary>
        /// The URL.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The image URL.
        /// </summary>
        public string Image { get; set; }
    }
}
