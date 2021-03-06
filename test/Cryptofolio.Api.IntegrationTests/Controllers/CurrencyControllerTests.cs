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
    public class CurrencyControllerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory _factory;

        private TestData Data => _factory.Data;

        public CurrencyControllerTests(WebApplicationFactory factory)
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
            context.Currencies.Add(Data.USD);
            context.SaveChanges();

            // Act
            var currency = await client.GetFromJsonAsync<Currency>($"/currencies/{Data.USD.Id}");

            // Assert
            currency.Should().BeEquivalentTo(Data.USD);
        }

        [Fact]
        public async Task Get_List_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            context.Currencies.AddRange(Data.USD, Data.EUR);
            context.SaveChanges();

            // Act
            var currencies = await client.GetFromJsonAsync<List<Currency>>("/currencies");

            // Assert
            currencies.Should().HaveCount(2);
            currencies.Should().ContainEquivalentOf(Data.USD);
            currencies.Should().ContainEquivalentOf(Data.EUR);
        }
    }
}
