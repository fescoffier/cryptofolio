using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Provides a JSON converter for <see cref="Transaction"/>.
    /// </summary>
    public class TransactionJsonConverter : JsonConverter<Transaction>
    {
        /// <inheritdoc/>
        public override Transaction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new NotImplementedException();

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, Transaction value, JsonSerializerOptions options)
        {
            if (value != null)
            {
                JsonSerializer.Serialize(writer, value, value.GetType(), options);
            }
        }
    }
}
