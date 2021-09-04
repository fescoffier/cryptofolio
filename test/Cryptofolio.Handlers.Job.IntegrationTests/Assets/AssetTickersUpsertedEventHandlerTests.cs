using Cryptofolio.Handlers.Job.Assets;
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

namespace Cryptofolio.Handlers.Job.IntegrationTests.Assets
{
    public class AssetTickersUpsertedEventHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly AssetTickersUpsertedEventHandler _handler;
        private readonly CurrencyTickerCache _cache;

        private TestData Data => _factory.Data;

        public AssetTickersUpsertedEventHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<AssetTickersUpsertedEventHandler>();
            _cache = _scope.ServiceProvider.GetRequiredService<CurrencyTickerCache>();
        }

        [Fact]
        public async Task Handle_Test()
        {
            // Setup
            var @event = new AssetTickersUpsertedEvent
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                UserId = Guid.NewGuid().ToString(),
                Tickers = new[]
                {
                    Data.BTC_USD_Ticker,
                    Data.BTC_EUR_Ticker,
                    Data.ETH_USD_Ticker,
                    Data.ETH_EUR_Ticker
                }
            };
            var cancellationToken = CancellationToken.None;

            // Act
            await _handler.Handle(@event, cancellationToken, null);

            // Assert
            var expectedTickers = new[]
            {
                new Ticker
                {
                    Pair = $"{Data.BTC_USD_Ticker.Asset.Symbol}/{Data.BTC_USD_Ticker.VsCurrency.Code}",
                    Timestamp = Data.BTC_USD_Ticker.Timestamp,
                    Value = Data.BTC_USD_Ticker.Value
                },
                new Ticker
                {
                    Pair = $"{Data.BTC_EUR_Ticker.Asset.Symbol}/{Data.BTC_EUR_Ticker.VsCurrency.Code}",
                    Timestamp = Data.BTC_EUR_Ticker.Timestamp,
                    Value = Data.BTC_EUR_Ticker.Value
                },
                new Ticker
                {
                    Pair = $"{Data.ETH_USD_Ticker.Asset.Symbol}/{Data.ETH_USD_Ticker.VsCurrency.Code}",
                    Timestamp = Data.ETH_USD_Ticker.Timestamp,
                    Value = Data.ETH_USD_Ticker.Value
                },
                new Ticker
                {
                    Pair = $"{Data.ETH_EUR_Ticker.Asset.Symbol}/{Data.ETH_EUR_Ticker.VsCurrency.Code}",
                    Timestamp = Data.ETH_EUR_Ticker.Timestamp,
                    Value = Data.ETH_EUR_Ticker.Value
                }
            };
            var tickers = await _cache.GetTickersAsync(expectedTickers.Select(t => t.Pair).ToArray());
            tickers.OrderBy(t => t.Timestamp).Should().BeEquivalentTo(expectedTickers.OrderBy(t => t.Timestamp));
        }
    }
}
