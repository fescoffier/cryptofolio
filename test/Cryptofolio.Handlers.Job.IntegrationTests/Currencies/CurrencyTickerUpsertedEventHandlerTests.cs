using Cryptofolio.Handlers.Job.Currencies;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Balances;
using Cryptofolio.Infrastructure.Caching;
using Cryptofolio.Infrastructure.Entities;
using Cryptofolio.Infrastructure.TestsCommon;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Handlers.Job.IntegrationTests.Currencies
{
    public class CurrencyTickerUpsertedEventHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly CurrencyTickersUpsertedEventHandler _handler;
        private readonly CurrencyTickerCache _cache;
        private readonly KafkaProducerWrapper<string, BulkComputeWalletBalanceRequest> _producerWrapper;
        private readonly KafkaConsumerWrapper<string, BulkComputeWalletBalanceRequest> _consumerWrapper;

        private TestData Data => _factory.Data;

        public CurrencyTickerUpsertedEventHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<CurrencyTickersUpsertedEventHandler>();
            _cache = _scope.ServiceProvider.GetRequiredService<CurrencyTickerCache>();
            _producerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaProducerWrapper<string, BulkComputeWalletBalanceRequest>>();
            _consumerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaConsumerWrapper<string, BulkComputeWalletBalanceRequest>>();
        }

        [Fact]
        public async Task Handle_Test()
        {
            // Setup
            var @event = new CurrencyTickersUpsertedEvent
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                UserId = Guid.NewGuid().ToString(),
                Tickers = new[]
                {
                    Data.USD_EUR_Ticker,
                    Data.EUR_USD_Ticker
                }
            };
            var cancellationToken = CancellationToken.None;
            _producerWrapper.Options.Topic = _consumerWrapper.Options.Topic = Guid.NewGuid().ToString();

            // Act
            await _handler.Handle(@event, cancellationToken, null);

            // Assert
            var expectedTickers = new[]
            {
                new Ticker
                {
                    Pair = $"{Data.USD_EUR_Ticker.Currency.Code}/{Data.USD_EUR_Ticker.VsCurrency.Code}",
                    Timestamp = Data.USD_EUR_Ticker.Timestamp,
                    Value = Data.USD_EUR_Ticker.Value
                },
                new Ticker
                {
                    Pair = $"{Data.EUR_USD_Ticker.Currency.Code}/{Data.EUR_USD_Ticker.VsCurrency.Code}",
                    Timestamp = Data.EUR_USD_Ticker.Timestamp,
                    Value = Data.EUR_USD_Ticker.Value
                }
            };
            var tickers = await _cache.GetTickersAsync(expectedTickers.Select(t => t.Pair).ToArray());
            tickers.Should().BeEquivalentTo(expectedTickers);
            _consumerWrapper.Consumer.Subscribe(_consumerWrapper.Options.Topic);
            var cr = _consumerWrapper.Consumer.Consume();
            _consumerWrapper.Consumer.Unsubscribe();
            cr.Message.Value.Should().BeEquivalentTo(new BulkComputeWalletBalanceRequest
            {
                CurrencyIds = new[]
                {
                    Data.USD.Id,
                    Data.EUR.Id
                }
            });
        }
    }
}
