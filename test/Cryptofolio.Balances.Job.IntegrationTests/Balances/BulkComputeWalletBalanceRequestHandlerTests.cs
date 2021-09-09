using Confluent.Kafka;
using Cryptofolio.Balances.Job.Balances;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Balances;
using Cryptofolio.Infrastructure.TestsCommon;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Balances.Job.IntegrationTests.Balances
{
    public class BulkComputeWalletBalanceRequestHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly BulkComputeWalletBalanceRequestHandler _handler;
        private readonly KafkaConsumerWrapper<string, ComputeWalletBalanceRequest> _consumerWrapper;
        private readonly CryptofolioContext _context;

        private TestData Data => _factory.Data;

        public BulkComputeWalletBalanceRequestHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<BulkComputeWalletBalanceRequestHandler>();
            _consumerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaConsumerWrapper<string, ComputeWalletBalanceRequest>>();
            _context = _scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
        }

        [Fact]
        public async Task Handle_Test()
        {
            // Setup
            _context.Transactions.AddRange(Data.Transaction1, Data.Transaction2, Data.Transaction3, Data.Transaction4);
            _context.SaveChanges();
            var request = new BulkComputeWalletBalanceRequest
            {
                AssetIds = new[]
                {
                    Data.BTC.Id,
                    Data.ETH.Id
                },
                CurrencyIds = new[]
                {
                    Data.USD.Id,
                    Data.EUR.Id
                }
            };
            var cancellationToken = CancellationToken.None;

            // Act
            await _handler.Handle(request, cancellationToken);

            // Assert
            var ids = new List<string>();
            _consumerWrapper.Consumer.Subscribe(_consumerWrapper.Options.Topic);
            ConsumeResult<string, ComputeWalletBalanceRequest> cr;
            do
            {
                cr = _consumerWrapper.Consumer.Consume();
            } while (!cr.IsPartitionEOF);
            _consumerWrapper.Consumer.Unsubscribe();
            ids.Should().BeEquivalentTo(new[] { Data.Wallet1.Id, Data.Wallet2.Id, Data.Wallet3.Id });
        }
    }
}
