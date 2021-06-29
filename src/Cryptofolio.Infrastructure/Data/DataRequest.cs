using MediatR;
using System;
using System.Text.Json.Serialization;

namespace Cryptofolio.Infrastructure.Data
{
    /// <summary>
    /// Models a date request.
    /// </summary>
    public abstract class DataRequest : IRequest
    {
        /// <summary>
        /// The identifier that can be used to correlate logs.
        /// </summary>
        [JsonPropertyName("trace_identifier")]
        public string TraceIdentifier { get; set; }

        /// <summary>
        /// The request date.
        /// </summary>
        [JsonPropertyName("date")]
        public DateTimeOffset Date { get; set; }
    }
}
