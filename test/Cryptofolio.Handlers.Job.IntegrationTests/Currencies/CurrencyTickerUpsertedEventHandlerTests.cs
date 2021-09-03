using Cryptofolio.Handlers.Job.Currencies;
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

        private TestData Data => _factory.Data;

        public CurrencyTickerUpsertedEventHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<CurrencyTickersUpsertedEventHandler>();
            _cache = _scope.ServiceProvider.GetRequiredService<CurrencyTickerCache>();
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
                    Data.USDTicker,
                    Data.EURTicker
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
                    Pair = $"{Data.USDTicker.Currency.Code}/{Data.USDTicker.VsCurrency.Code}",
                    Timestamp = Data.USDTicker.Timestamp,
                    Value = Data.USDTicker.Value
                },
                new Ticker
                {
                    Pair = $"{Data.EURTicker.Currency.Code}/{Data.EURTicker.VsCurrency.Code}",
                    Timestamp = Data.EURTicker.Timestamp,
                    Value = Data.EURTicker.Value
                },
            };
            var tickers = await _cache.GetTickersAsync(expectedTickers.Select(t => t.Pair).ToArray());
            tickers.Should().BeEquivalentTo(expectedTickers);
        }
    }
}
