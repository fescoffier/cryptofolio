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
    public class ExchangeDataRequestHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly IServiceScope _scope;
        private readonly ExchangeDataRequestHandler _handler;
        private readonly IExchangesClient _exchangesClient;
        private readonly CryptofolioContext _context;

        public ExchangeDataRequestHandlerTests(WebApplicationFactory factory)
        {
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<ExchangeDataRequestHandler>();
            _exchangesClient = _scope.ServiceProvider.GetRequiredService<IExchangesClient>();
            _context = _scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
        }

        [Fact]
        public async Task Handle_Test()
        {
            // Setup
            var exchangeId = "gdax";
            var gdax = await _exchangesClient.GetExchangesByExchangeId(exchangeId);
            var request = new ExchangeDataRequest
            {
                TraceIdentifier = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                Id = exchangeId
            };

            // Act
            await _handler.Handle(request, CancellationToken.None, null);

            // Assert
            var exchange = _context.Exchanges.SingleOrDefault(a => a.Id == exchangeId);
            exchange.Id.Should().Be(exchangeId);
            exchange.Name.Should().Be(gdax.Name);
            exchange.Description.Should().Be(gdax.Description);
            exchange.YearEstablished.Should().Be(gdax.YearEstablished);
            exchange.Url.Should().Be(gdax.Url);
            exchange.Image.Should().Be(gdax.Image);
        }

        [Fact]
        public void Handle_RequestCancelled_Test()
        {
            // Setup
            var exchangeId = "gdax";
            var request = new ExchangeDataRequest
            {
                TraceIdentifier = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                Id = exchangeId
            };
            var cancellationToken = new CancellationToken(true);

            // Act
            _handler.Awaiting(h => h.Handle(request, cancellationToken, null)).Should().Throw<OperationCanceledException>();
        }
    }
}
