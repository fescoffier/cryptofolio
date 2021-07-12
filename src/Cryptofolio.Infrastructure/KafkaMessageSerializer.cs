using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text.Json;

namespace Cryptofolio.Infrastructure
{
    /// <inheritdoc/>
    public class KafkaMessageSerializer<TMessage> : ISerializer<TMessage>, IDeserializer<TMessage>
    {
        private readonly IOptionsMonitor<KafkaOptions<TMessage>> _optionsMonitor;

        private KafkaOptions<TMessage> Options => _optionsMonitor.CurrentValue;

        /// <summary>
        /// Creates a new instance of <see cref="KafkaMessageSerializer{TMessage}"/>.
        /// </summary>
        /// <param name="optionsMonitor">The options monitor.</param>
        public KafkaMessageSerializer(IOptionsMonitor<KafkaOptions<TMessage>> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }

        /// <inheritdoc/>
        public byte[] Serialize(TMessage data, SerializationContext context)
        {
            using var ms = new MemoryStream();
            using var writer = new Utf8JsonWriter(ms);
            JsonSerializer.Serialize<TMessage>(writer, data, Options.ValueSerilializerOptions);
            return ms.ToArray();
        }

        /// <inheritdoc/>
        public TMessage Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            var reader = new Utf8JsonReader(data);
            return JsonSerializer.Deserialize<TMessage>(ref reader, Options.ValueSerilializerOptions);
        }
    }
}
