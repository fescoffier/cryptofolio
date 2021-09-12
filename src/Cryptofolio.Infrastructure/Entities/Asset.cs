using System.Text.Json.Serialization;

namespace Cryptofolio.Infrastructure.Entities
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
        /// The current value.
        /// </summary>
        public decimal CurrentValue { get; set; }

        /// <summary>
        /// The description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The thumb image URL.
        /// </summary>
        [JsonPropertyName("thumb_image_url")]
        public string ThumbImageUrl { get; set; }

        /// <summary>
        /// The small image URL.
        /// </summary>
        [JsonPropertyName("small_image_url")]
        public string SmallImageUrl { get; set; }

        /// <summary>
        /// The large image URL.
        /// </summary>
        [JsonPropertyName("large_image_url")]
        public string LargeImageUrl { get; set; }
    }
}
