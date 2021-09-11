using Cryptofolio.Collector.Job.Fixer;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Collector.Job.IntegrationTests.Fixer
{
    public class FixerClientTests : IClassFixture<WebApplicationFactory>
    {
        private readonly IServiceScope _scope;
        private readonly FixerClient _client;

        public FixerClientTests(WebApplicationFactory factory)
        {
            _scope = factory.Services.CreateScope();
            _client = _scope.ServiceProvider.GetRequiredService<FixerClient>();
        }

        [Fact]
        public async Task GetLatestRatesAsync_Test()
        {
            // Setup
            var currency = "usd";
            var vsCurrencies = new[] { "eur", "gbp" };
            var cancellationToken = CancellationToken.None;

            // Act
            var rates = await _client.GetLatestRatesAsync(currency, vsCurrencies, cancellationToken);

            // Assert
            rates.Success.Should().BeTrue();
            rates.Error.Should().BeNull();
            rates.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, precision: TimeSpan.FromMinutes(10));
            rates.Date.Should().Be(DateTime.UtcNow.Date);
            rates.Base.Should().Be("USD");
            rates.Rates.Should().ContainKey("EUR").WhoseValue.Should().BeGreaterThan(0);
            rates.Rates.Should().ContainKey("GBP").WhoseValue.Should().BeGreaterThan(0);
        }
    }
}
