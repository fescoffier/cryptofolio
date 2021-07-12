using System.Text.Json.Serialization;

namespace Cryptofolio.Infrastructure.Data
{
    /// <summary>
    /// Models a data request for <see cref="Core.Entities.Exchange"/>.
    /// </summary>
    public class ExchangeDataRequest : DataRequest
    {
        /// <summary>
        /// The exchange id.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}
