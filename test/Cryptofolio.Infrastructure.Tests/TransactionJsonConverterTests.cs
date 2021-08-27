using Cryptofolio.Infrastructure.Entities;
using Cryptofolio.Infrastructure.TestsCommon;
using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace Cryptofolio.Infrastructure.Tests
{
    public class TransactionJsonConverterTests
    {
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new TransactionJsonConverter()
            }
        };
        private readonly JsonSerializerOptions _deserializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        private readonly TestData _data = new();

        [Fact]
        public void Test()
        {
            // Act
            var buyTransactionJson = JsonSerializer.Serialize(_data.Transaction1, _serializerOptions);
            var buyTransaction = JsonSerializer.Deserialize<BuyOrSellTransaction>(buyTransactionJson, _deserializerOptions);
            var sellTransactionJson = JsonSerializer.Serialize<Transaction>(_data.Transaction2, _serializerOptions);
            var sellTransaction = JsonSerializer.Deserialize<BuyOrSellTransaction>(sellTransactionJson, _deserializerOptions);
            var transferTransactionJson = JsonSerializer.Serialize<Transaction>(_data.Transaction4, _serializerOptions);
            var transferTransaction = JsonSerializer.Deserialize<TransferTransaction>(transferTransactionJson, _deserializerOptions);

            // Assert
            buyTransaction.Should().BeEquivalentTo(_data.Transaction1);
            sellTransaction.Should().BeEquivalentTo(_data.Transaction2);
            transferTransaction.Should().BeEquivalentTo(_data.Transaction4);
        }
    }
}
