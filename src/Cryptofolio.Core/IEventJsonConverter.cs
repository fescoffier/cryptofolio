using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cryptofolio.Core
{
    /// <summary>
    /// Provides a JSON converter for <see cref="IEvent"/>.
    /// </summary>
    public class IEventJsonConverter : JsonConverter<IEvent>
    {
        private const string EventTypePropertyName = "EventType";
        private const string EventPropertyName = "Event";

        /// <inheritdoc/>
        public override IEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            while (reader.TokenType != JsonTokenType.PropertyName)
            {
                reader.Read();
            }
            reader.Read();
            var eventType = Type.GetType(reader.GetString());
            reader.Read();
            reader.Read();
            var @event = (IEvent)JsonSerializer.Deserialize(ref reader, eventType, options);
            reader.Read();
            return @event;
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, IEvent value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            var valueType = value.GetType();
            writer.WriteString(EventTypePropertyName, valueType.AssemblyQualifiedName);
            writer.WritePropertyName(EventPropertyName);
            JsonSerializer.Serialize(writer, value, valueType, options);
            writer.WriteEndObject();
        }
    }
}
