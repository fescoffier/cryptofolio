using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Entities;
using Cryptofolio.Infrastructure.TestsCommon;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Api.IntegrationTests.Controllers
{
    public class AssetControllerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory _factory;

        private TestData Data => _factory.Data;

        public AssetControllerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            factory.PurgeData();
        }

        [Fact]
        public async Task Get_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            context.Assets.Add(Data.BTC);
            context.SaveChanges();

            // Act
            var asset = await client.GetFromJsonAsync<Asset>($"/assets/{Data.BTC.Id}");

            // Assert
            asset.Should().BeEquivalentTo(Data.BTC);
        }

        [Fact]
        public async Task Get_List_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            context.Assets.AddRange(Data.BTC, Data.ETH);
            context.SaveChanges();

            // Act
            var assets = await client.GetFromJsonAsync<List<Asset>>("/assets");

            // Assert
            assets.Should().HaveCount(2);
            assets.Should().ContainEquivalentOf(Data.BTC);
            assets.Should().ContainEquivalentOf(Data.ETH);
        }

        [Fact]
        public async Task Sources_Test()
        {
            // Setup
            var client = _factory.CreateClient();

            // Act
            var sources = await client.GetFromJsonAsync<IEnumerable<string>>("/assets/sources");

            // Assert
            sources.Should().HaveCount(3);
            sources.Should().BeEquivalentTo(InfrastructureConstants.Transactions.Sources.All);
        }

        [Fact]
        public async Task Destinations_Test()
        {
            // Setup
            var client = _factory.CreateClient();

            // Act
            var sources = await client.GetFromJsonAsync<IEnumerable<string>>("/assets/destinations");

            // Assert
            sources.Should().HaveCount(3);
            sources.Should().BeEquivalentTo(InfrastructureConstants.Transactions.Destinations.All);
        }
    }
}
