using System.Collections.Generic;
using System.Text.Json;

namespace Cryptofolio.Infrastructure
{
    /// <summary>
    /// Configuration options for Elasticsearch.
    /// </summary>
    public class ElasticsearchOptions
    {
        /// <summary>
        /// The server nodes.
        /// </summary>
        public IEnumerable<string> Nodes { get; set; }

        /// <summary>
        /// The indices to document map.
        /// </summary>
        public Dictionary<string, string> Indices { get; set; }

        /// <summary>
        /// The JSON serializer options.
        /// </summary>
        public JsonSerializerOptions SerializationOptions { get; set; }
    }
}
