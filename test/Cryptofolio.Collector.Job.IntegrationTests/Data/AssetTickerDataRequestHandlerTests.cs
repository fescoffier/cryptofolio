using CoinGecko.Interfaces;
using Cryptofolio.Collector.Job.Data;
using Cryptofolio.Core.Entities;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Collector.Job.IntegrationTests.Data
{
    public class AssetTickerDataRequestHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly IServiceScope _scope;
        private readonly AssetTickerDataRequestHandler _handler;
        private readonly ISimpleClient _simpleClient;
        private readonly CryptofolioContext _context;

        public AssetTickerDataRequestHandlerTests(WebApplicationFactory factory)
        {
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<AssetTickerDataRequestHandler>();
            _simpleClient = _scope.ServiceProvider.GetRequiredService<ISimpleClient>();
            _context = _scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
        }

        [Fact]
        public async Task Handle_Test()
        {
            // Setup
            var bitcoin = new Asset
            {
                Id = "bitcoin",
                Name = "Bitcoin",
                Symbol = "BTC",
                Description = "Lorem ipsum dolor sit amet."
            };
            var ethereum = new Asset
            {
                Id = "ethereum",
                Name = "Ethereum",
                Symbol = "ETH",
                Description = "Lorem ipsum dolor sit amet."
            };
            var usd = "usd";
            var eur = "eur";
            var vsCurrencies = new[] { usd, eur };
            var request = new AssetTickerDataRequest
            {
                TraceIdentifier = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                Ids = new[]
                {
                    bitcoin.Id,
                    ethereum.Id
                },
                VsCurrencies = vsCurrencies
            };
            _context.Assets.AddRange(bitcoin, ethereum);
            _context.SaveChanges();

            // Act
            await _handler.Handle(request, CancellationToken.None, null);

            // Assert
            var price = await _simpleClient.GetSimplePrice(new[] { bitcoin.Id, ethereum.Id }, vsCurrencies, false, false, false, true);
            var tickers = _context.AssetTickers.ToList();
            // Bitcoin
            tickers.Single(t => t.Asset.Id == bitcoin.Id && t.VsCurrency == usd).Timestamp.ToUnixTimeSeconds().Should().Be(Convert.ToInt64(price[bitcoin.Id]["last_updated_at"]));
            tickers.Single(t => t.Asset.Id == bitcoin.Id && t.VsCurrency == usd).Value.Should().Be(price[bitcoin.Id][usd]);
            tickers.Single(t => t.Asset.Id == bitcoin.Id && t.VsCurrency == eur).Timestamp.ToUnixTimeSeconds().Should().Be(Convert.ToInt64(price[bitcoin.Id]["last_updated_at"]));
            tickers.Single(t => t.Asset.Id == bitcoin.Id && t.VsCurrency == eur).Value.Should().Be(price[bitcoin.Id][eur]);
            // Ethereum
            tickers.Single(t => t.Asset.Id == ethereum.Id && t.VsCurrency == usd).Timestamp.ToUnixTimeSeconds().Should().Be(Convert.ToInt64(price[ethereum.Id]["last_updated_at"]));
            tickers.Single(t => t.Asset.Id == ethereum.Id && t.VsCurrency == usd).Value.Should().Be(price[ethereum.Id][usd]);
            tickers.Single(t => t.Asset.Id == ethereum.Id && t.VsCurrency == eur).Timestamp.ToUnixTimeSeconds().Should().Be(Convert.ToInt64(price[ethereum.Id]["last_updated_at"]));
            tickers.Single(t => t.Asset.Id == ethereum.Id && t.VsCurrency == eur).Value.Should().Be(price[ethereum.Id][eur]);
        }

        [Fact]
        public void Handle_RequestCancelled_Test()
        {
            // Setup
            var request = new AssetTickerDataRequest
            {
                TraceIdentifier = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                Ids = new[]
                {
                    "bitcoin",
                    "ethereum"
                },
                VsCurrencies = new[]
                {
                    "usd",
                    "eur"
                }
            };
            var cancellationToken = new CancellationToken(true);

            // Act
            _handler.Awaiting(h => h.Handle(request, cancellationToken, null)).Should().Throw<OperationCanceledException>();
        }
    }
}
