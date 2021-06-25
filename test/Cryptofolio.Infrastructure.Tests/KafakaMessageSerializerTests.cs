using Confluent.Kafka;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using Xunit;

namespace Cryptofolio.Infrastructure.Tests
{
    public class KafakaMessageSerializerTests
    {
        private const string SerializedDataBase64 = "eyJJZCI6ImU2YzNlYmZiLWY0ODEtNGI0ZS1hOTU3LTdiNWNkNzgyNmM4MSIsIk1lc3NhZ2UiOiJMb3JlbSBpcHN1bSBkb2xvciBzaXQgYW1ldCJ9";

        [Fact]
        public void Serialize_Test()
        {
            // Setup
            var serializerOptions = new KafkaOptions<TestMessage>
            {
                ValueSerilializerOptions = new()
            };
            var serializerOptionsMonitorMock = new Mock<IOptionsMonitor<KafkaOptions<TestMessage>>>();
            serializerOptionsMonitorMock.SetupGet(m => m.CurrentValue).Returns(serializerOptions);
            var serializer = new KafkaMessageSerializer<TestMessage>(serializerOptionsMonitorMock.Object);
            var data = new TestMessage
            {
                Id = "e6c3ebfb-f481-4b4e-a957-7b5cd7826c81",
                Message = "Lorem ipsum dolor sit amet"
            };
            var context = SerializationContext.Empty;

            // Act
            var bytes = serializer.Serialize(data, context);

            // Assert
            Convert.ToBase64String(bytes).Should().Be(SerializedDataBase64);
        }

        [Fact]
        public void Deserialize_Test()
        {
            // Setup
            var serializerOptions = new KafkaOptions<TestMessage>
            {
                ValueSerilializerOptions = new()
            };
            var serializerOptionsMonitorMock = new Mock<IOptionsMonitor<KafkaOptions<TestMessage>>>();
            serializerOptionsMonitorMock.SetupGet(m => m.CurrentValue).Returns(serializerOptions);
            var serializer = new KafkaMessageSerializer<TestMessage>(serializerOptionsMonitorMock.Object);
            var serializedData = new ReadOnlySpan<byte>(Convert.FromBase64String(SerializedDataBase64));
            var context = SerializationContext.Empty;

            // Act
            var data = serializer.Deserialize(serializedData, false, context);

            // Assert
            data.Should().BeEquivalentTo(new TestMessage
            {
                Id = "e6c3ebfb-f481-4b4e-a957-7b5cd7826c81",
                Message = "Lorem ipsum dolor sit amet"
            });
        }

        public class TestMessage
        {
            public string Id { get; set; }

            public string Message { get; set; }
        }
    }
}
