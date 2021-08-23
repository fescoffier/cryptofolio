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
    public class ExchangeControllerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory _factory;

        private TestData Data => _factory.Data;

        public ExchangeControllerTests(WebApplicationFactory factory)
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
            context.Exchanges.Add(Data.Exchange1);
            context.SaveChanges();

            // Act
            var transaction = await client.GetFromJsonAsync<Exchange>($"/exchanges/{Data.Exchange1.Id}");

            // Assert
            transaction.Should().BeEquivalentTo(Data.Exchange1);
        }

        [Fact]
        public async Task Get_List_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            context.Exchanges.AddRange(Data.Exchange1, Data.Exchange2);
            context.SaveChanges();

            // Act
            var transactions = await client.GetFromJsonAsync<List<Exchange>>("/exchanges");

            // Assert
            transactions.Should().HaveCount(2);
            transactions.Should().ContainEquivalentOf(Data.Exchange1);
            transactions.Should().ContainEquivalentOf(Data.Exchange2);
        }
    }
}
