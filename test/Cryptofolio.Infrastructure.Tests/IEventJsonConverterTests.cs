using FluentAssertions;
using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using Xunit;

namespace Cryptofolio.Infrastructure.Tests
{
    public class IEventJsonConverterTests
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            Converters =
            {
                new IEventJsonConverter()
            },
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        private static readonly FakeEvent Event = new()
        {
            Id = "407db215-9d50-40f3-8a88-eff521667131",
            Date = DateTimeOffset.Parse("2021-06-30T21:56:16.7007267+00:00"),
            UserId = "0f5ab98a-dd5d-496c-afc0-7bbc38e6a6cb",
            Property1 = "value1",
            Property2 = "value2"
        };
        private const string EventJson = "{\"EventType\":\"Cryptofolio.Infrastructure.Tests.IEventJsonConverterTests+FakeEvent, Cryptofolio.Infrastructure.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\",\"Event\":{\"Id\":\"407db215-9d50-40f3-8a88-eff521667131\",\"Date\":\"2021-06-30T21:56:16.7007267+00:00\",\"UserId\":\"0f5ab98a-dd5d-496c-afc0-7bbc38e6a6cb\",\"Category\":\"Test\",\"Property1\":\"value1\",\"Property2\":\"value2\"}}";

        [Fact]
        public void Serialize_Test()
        {
            // Act
            var json = JsonSerializer.Serialize<IEvent>(Event, SerializerOptions);

            // Assert
            json.Should().Be(EventJson);
        }

        [Fact]
        public void Deserialize_Test()
        {
            // Act
            var @event = JsonSerializer.Deserialize<IEvent>(EventJson, SerializerOptions);

            // Assert
            @event.Should().BeEquivalentTo(Event);
        }

        public class FakeEvent : IEvent
        {
            public string Id { get; init; }

            public DateTimeOffset Date { get; init; }

            public string UserId { get; init; }

            public string Category => "Test";

            public string Property1 { get; init; }

            public string Property2 { get; init; }
        }
    }
}
