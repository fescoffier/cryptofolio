using System.Text.Json;

namespace Cryptofolio.Collector.Job.Fixer
{
    /// <summary>
    /// Configuration options for the <see cref="FixerClient"/>.
    /// </summary>
    public class FixerOptions
    {
        /// <summary>
        /// The API URL.
        /// </summary>
        public string ApiUri { get; set; }

        /// <summary>
        /// The API key.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// The "latest" API endpoint.
        /// </summary>
        public string LatestEndpoint => $"{ApiUri}/latest";

        /// <summary>
        /// The JSON serializer options used to serialize/deserialize API requests/responses.
        /// </summary>
        public JsonSerializerOptions SerializerOptions { get; set; }
    }
}
