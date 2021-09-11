using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Provides a polymorphic JSON converter for <see cref="Transaction"/>.
    /// </summary>
    public class TransactionPolymorphicJsonConverter : JsonConverter<Transaction>
    {
        private const string TypePropertyName = "type";
        private const string ValuePropertyName = "value";

        /// <inheritdoc/>
        public override Transaction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();
            reader.Read();
            var type = Type.GetType(reader.GetString());
            reader.Read();
            reader.Read();
            var value = JsonSerializer.Deserialize(ref reader, type, options);
            reader.Read();
            return value as Transaction;
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, Transaction value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(TypePropertyName, value.GetType().AssemblyQualifiedName);
            writer.WritePropertyName(ValuePropertyName);
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
            writer.WriteEndObject();
        }
    }
}
