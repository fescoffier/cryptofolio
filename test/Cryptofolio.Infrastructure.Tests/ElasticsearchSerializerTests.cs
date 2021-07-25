using FluentAssertions;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Infrastructure.Tests
{
    public class ElasticsearchSerializerTests
    {
        private readonly JsonSerializerOptions _options;
        private readonly ElasticsearchSerializer _serializer;

        public ElasticsearchSerializerTests()
        {
            _options = new();
            _serializer = new(_options);
        }

        [Fact]
        public void Deserialize_NonGeneric_Test()
        {
            // Setup
            var type = typeof(FakeObject);
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(FakeObject.Json));

            // Act
            var obj = _serializer.Deserialize(type, ms);

            // Assert
            obj.Should().BeEquivalentTo(FakeObject.Instance);
        }

        [Fact]
        public void Deserialize_Generic_Test()
        {
            // Setup
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(FakeObject.Json));

            // Act
            var obj = _serializer.Deserialize<FakeObject>(ms);

            // Assert
            obj.Should().BeEquivalentTo(FakeObject.Instance);
        }

        [Fact]
        public async Task DeserializeAsync_NonGeneric_Test()
        {
            // Setup
            var type = typeof(FakeObject);
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(FakeObject.Json));

            // Act
            var obj = await _serializer.DeserializeAsync(type, ms);

            // Assert
            obj.Should().BeEquivalentTo(FakeObject.Instance);
        }

        [Fact]
        public async Task DeserializeAsync_Generic_Test()
        {
            // Setup
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(FakeObject.Json));

            // Act
            var obj = await _serializer.DeserializeAsync<FakeObject>(ms);

            // Assert
            obj.Should().BeEquivalentTo(FakeObject.Instance);
        }

        [Fact]
        public void Serialize_Test()
        {
            // Setup
            var obj = FakeObject.Instance;
            using var ms = new MemoryStream();

            // Act
            _serializer.Serialize(obj, ms, default);

            // Assert
            Encoding.UTF8.GetString(ms.ToArray()).Should().Be(FakeObject.Json);
        }

        [Fact]
        public async Task SerializeAsync_Test()
        {
            // Setup
            var obj = FakeObject.Instance;
            using var ms = new MemoryStream();

            // Act
            await _serializer.SerializeAsync(obj, ms, default);

            // Assert
            Encoding.UTF8.GetString(ms.ToArray()).Should().Be(FakeObject.Json);
        }

        private class FakeObject
        {
            public static FakeObject Instance { get; } = new FakeObject
            {
                Property1 = "Value1",
                Property2 = "Value2"
            };

            public static string Json { get; } = "{\"Property1\":\"Value1\",\"Property2\":\"Value2\"}";

            public string Property1 { get; set; }

            public string Property2 { get; set; }
        }
    }
}
