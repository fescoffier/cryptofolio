using Cryptofolio.Infrastructure.Entities;
using Cryptofolio.Infrastructure.TestsCommon;
using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace Cryptofolio.Infrastructure.Tests
{
    public class TransactionPolymorphicJsonConverterTests
    {
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new TransactionPolymorphicJsonConverter()
            }
        };
        private readonly TestData _data = new();

        [Fact]
        public void Test()
        {
            // Act
            var buyTransactionJson = JsonSerializer.Serialize<Transaction>(_data.Transaction1, _serializerOptions);
            var buyTransaction = JsonSerializer.Deserialize<Transaction>(buyTransactionJson, _serializerOptions);
            var sellTransactionJson = JsonSerializer.Serialize<Transaction>(_data.Transaction2, _serializerOptions);
            var sellTransaction = JsonSerializer.Deserialize<Transaction>(sellTransactionJson, _serializerOptions);
            var transferTransactionJson = JsonSerializer.Serialize<Transaction>(_data.Transaction4, _serializerOptions);
            var transferTransaction = JsonSerializer.Deserialize<Transaction>(transferTransactionJson, _serializerOptions);

            // Assert
            buyTransaction.Should().BeEquivalentTo(_data.Transaction1);
            sellTransaction.Should().BeEquivalentTo(_data.Transaction2);
            transferTransaction.Should().BeEquivalentTo(_data.Transaction4);
        }
    }
}
