using Elasticsearch.Net;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Infrastructure
{
#pragma warning disable CA2012 // Use ValueTasks correctly
    /// <summary>
    /// Provides an implementation of <see cref="IElasticsearchSerializer"/> using the <see cref="JsonSerializer"/> implementation.
    /// </summary>
    public class ElasticsearchSerializer : IElasticsearchSerializer
    {
        private readonly JsonSerializerOptions _serializerOptions;

        /// <summary>
        /// Creates a new instance of <see cref="ElasticsearchSerializer"/>.
        /// </summary>
        /// <param name="serializerOptions">The serializer options.</param>
        public ElasticsearchSerializer(JsonSerializerOptions serializerOptions)
        {
            _serializerOptions = serializerOptions;
        }

        /// <inheritdoc/>
        public object Deserialize(Type type, Stream stream) =>
            JsonSerializer.DeserializeAsync(stream, type, _serializerOptions, CancellationToken.None)
                          .ConfigureAwait(false)
                          .GetAwaiter()
                          .GetResult();

        /// <inheritdoc/>
        public T Deserialize<T>(Stream stream) =>
            JsonSerializer.DeserializeAsync<T>(stream, _serializerOptions, CancellationToken.None)
                          .ConfigureAwait(false)
                          .GetAwaiter()
                          .GetResult();

        /// <inheritdoc/>
        public Task<object> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken = default) =>
            JsonSerializer.DeserializeAsync(stream, type, _serializerOptions, cancellationToken).AsTask();

        /// <inheritdoc/>
        public Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default) =>
            JsonSerializer.DeserializeAsync<T>(stream, _serializerOptions, cancellationToken).AsTask();

        /// <inheritdoc/>
        public void Serialize<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None) =>
            JsonSerializer.SerializeAsync(stream, data, _serializerOptions, CancellationToken.None)
                          .ConfigureAwait(false)
                          .GetAwaiter()
                          .GetResult();

        /// <inheritdoc/>
        public Task SerializeAsync<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None, CancellationToken cancellationToken = default) =>
            JsonSerializer.SerializeAsync(stream, data, _serializerOptions, cancellationToken);
    }
#pragma warning restore CA2012 // Use ValueTasks correctly
}
