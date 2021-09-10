using System.Text.Json.Serialization;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Models a fiat currency.
    /// </summary>
    public class Currency
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
        /// The code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The symbol.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// The roungind precision.
        /// </summary>
        public int Precision { get; set; } = 2;

        /// <summary>
        /// The value format.
        /// </summary>
        [JsonPropertyName("value_format")]
        public string ValueFormat { get; set; }
    }
}
