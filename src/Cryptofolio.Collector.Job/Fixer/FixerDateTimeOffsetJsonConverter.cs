using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cryptofolio.Collector.Job.Fixer
{
    /// <summary>
    /// Privides a JSON converter for <see cref="DateTimeOffset"/> deserialization from the Fixer API. 
    /// </summary>
    public class FixerDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
    {
        /// <inheritdoc/>
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            DateTimeOffset.FromUnixTimeSeconds(reader.GetInt32());

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) =>
            throw new NotImplementedException();
    }
}
