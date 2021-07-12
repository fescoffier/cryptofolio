using System.Text.Json.Serialization;

namespace Cryptofolio.Infrastructure.Data
{
    /// <summary>
    /// Models a data request for a <see cref="Core.Entities.Asset"/>;
    /// </summary>
    public class AssetDataRequest : DataRequest
    {
        /// <summary>
        /// The asset id.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}
