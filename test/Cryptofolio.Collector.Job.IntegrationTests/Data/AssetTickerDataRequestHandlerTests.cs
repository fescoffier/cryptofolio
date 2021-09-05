using CoinGecko.Interfaces;
using Cryptofolio.Collector.Job.Data;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Data;
using Cryptofolio.Infrastructure.TestsCommon;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Collector.Job.IntegrationTests.Data
{
    public class AssetTickerDataRequestHandlerTests : IClassFixture<WebApplicationFactory>, IDisposable
    {
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly AssetTickerDataRequestHandler _handler;
        private readonly ISimpleClient _simpleClient;
        private readonly CryptofolioContext _context;

        private TestData Data => _factory.Data;

        public AssetTickerDataRequestHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<AssetTickerDataRequestHandler>();
            _simpleClient = _scope.ServiceProvider.GetRequiredService<ISimpleClient>();
            _context = _scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
        }

        public void Dispose() => _scope.Dispose();

        [Fact]
        public async Task Handle_Test()
        {
            // Setup
            var ids = new[]
            {
                Data.BTC.Id,
                Data.ETH.Id
            };
            var vsCurrencies = new[]
            {
                Data.USD.Code,
                Data.EUR.Code
            };
            var request = new AssetTickerDataRequest
            {
                TraceIdentifier = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                Ids = ids,
                VsCurrencies = vsCurrencies
            };
            _context.Assets.AddRange(Data.BTC, Data.ETH);
            _context.Currencies.AddRange(Data.USD, Data.EUR);
            _context.SaveChanges();

            // Act
            await _handler.Handle(request, CancellationToken.None, null);

            // Assert
            var price = await _simpleClient.GetSimplePrice(ids, vsCurrencies, false, false, false, true);
            var tickers = _context.AssetTickers
                .Include(t => t.Asset)
                .Include(t => t.VsCurrency)
                .ToList();
            // Bitcoin
            tickers.Single(t => t.Asset.Id == Data.BTC.Id && t.VsCurrency.Id == Data.USD.Id).Timestamp.ToUnixTimeSeconds().Should().Be(Convert.ToInt64(price[Data.BTC.Id]["last_updated_at"]));
            tickers.Single(t => t.Asset.Id == Data.BTC.Id && t.VsCurrency.Id == Data.USD.Id).Value.Should().Be(price[Data.BTC.Id][Data.USD.Code]);
            tickers.Single(t => t.Asset.Id == Data.BTC.Id && t.VsCurrency.Id == Data.EUR.Id).Timestamp.ToUnixTimeSeconds().Should().Be(Convert.ToInt64(price[Data.BTC.Id]["last_updated_at"]));
            tickers.Single(t => t.Asset.Id == Data.BTC.Id && t.VsCurrency.Id == Data.EUR.Id).Value.Should().Be(price[Data.BTC.Id][Data.EUR.Code]);
            // Ethereum
            tickers.Single(t => t.Asset.Id == Data.ETH.Id && t.VsCurrency.Id == Data.USD.Id).Timestamp.ToUnixTimeSeconds().Should().Be(Convert.ToInt64(price[Data.ETH.Id]["last_updated_at"]));
            tickers.Single(t => t.Asset.Id == Data.ETH.Id && t.VsCurrency.Id == Data.USD.Id).Value.Should().Be(price[Data.ETH.Id][Data.USD.Code]);
            tickers.Single(t => t.Asset.Id == Data.ETH.Id && t.VsCurrency.Id == Data.EUR.Id).Timestamp.ToUnixTimeSeconds().Should().Be(Convert.ToInt64(price[Data.ETH.Id]["last_updated_at"]));
            tickers.Single(t => t.Asset.Id == Data.ETH.Id && t.VsCurrency.Id == Data.EUR.Id).Value.Should().Be(price[Data.ETH.Id][Data.EUR.Code]);
        }

        [Fact]
        public async Task Handle_RequestCancelled_Test()
        {
            // Setup
            var request = new AssetTickerDataRequest
            {
                TraceIdentifier = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                Ids = new[]
                {
                    Data.BTC.Id,
                    Data.ETH.Id
                },
                VsCurrencies = new[]
                {
                    Data.USD.Code,
                    Data.EUR.Code
                }
            };
            var cancellationToken = new CancellationToken(true);

            // Act
            await _handler.Awaiting(h => h.Handle(request, cancellationToken, null)).Should().ThrowAsync<OperationCanceledException>();
        }
    }
}
