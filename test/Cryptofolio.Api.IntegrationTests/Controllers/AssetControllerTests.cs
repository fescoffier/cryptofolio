using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Entities;
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
            var transaction = await client.GetFromJsonAsync<Asset>($"/assets/{Data.BTC.Id}");

            // Assert
            transaction.Should().BeEquivalentTo(Data.BTC);
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
            var transactions = await client.GetFromJsonAsync<List<Asset>>("/assets");

            // Assert
            transactions.Should().HaveCount(2);
            transactions.Should().ContainEquivalentOf(Data.BTC);
            transactions.Should().ContainEquivalentOf(Data.ETH);
        }
    }
}
