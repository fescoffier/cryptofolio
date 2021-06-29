using CoinGecko.Interfaces;
using Cryptofolio.Collector.Job.Data;
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
    public class AssetDataRequestHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly AssetDataRequestHandler _handler;
        private readonly ICoinsClient _coinsClient;
        private readonly CryptofolioContext _context;

        public AssetDataRequestHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<AssetDataRequestHandler>();
            _coinsClient = _scope.ServiceProvider.GetRequiredService<ICoinsClient>();
            _context = _scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
        }

        [Fact]
        public async Task Handle_Test()
        {
            // Setup
            var assetId = "ethereum";
            var eth = await _coinsClient.GetAllCoinDataWithId(assetId);
            var request = new AssetDataRequest
            {
                TraceIdentifier = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                Id = assetId
            };

            // Act
            await _handler.Handle(request, CancellationToken.None, null);

            // Assert
            var asset = _context.Assets.SingleOrDefault(a => a.Id == assetId);
            asset.Id.Should().Be(eth.Id);
            asset.Name.Should().Be(eth.Name);
            asset.Symbol.Should().Be(eth.Symbol);
            asset.Description.Should().Be(eth.Description["en"]);
        }

        [Fact]
        public void Handle_RequestCancelled_Test()
        {
            // Setup
            var assetId = "ethereum";
            var request = new AssetDataRequest
            {
                TraceIdentifier = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                Id = assetId
            };
            var cancellationToken = new CancellationToken(true);

            // Act
            _handler.Awaiting(h => h.Handle(request, cancellationToken, null)).Should().Throw<OperationCanceledException>();
        }
    }
}
