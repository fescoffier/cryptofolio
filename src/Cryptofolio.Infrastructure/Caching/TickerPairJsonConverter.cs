using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cryptofolio.Infrastructure.Caching
{
    /// <summary>
    /// Provides a JSON converter for <see cref="TickerPair"/>.
    /// </summary>
    public class TickerPairJsonConverter : JsonConverter<TickerPair>
    {
        /// <inheritdoc/>
        public override TickerPair Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetString();

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, TickerPair value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
    }
}
