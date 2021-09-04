using Cryptofolio.Collector.Job.Data;
using Cryptofolio.Collector.Job.Fixer;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Data;
using Cryptofolio.Infrastructure.TestsCommon;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Collector.Job.IntegrationTests.Data
{
    public class CurrencyTickerDataRequestHandlerTests : IClassFixture<WebApplicationFactory>, IDisposable
    {
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly CurrencyTickerDataRequestHandler _handler;
        private readonly FixerClient _fixerClient;
        private readonly CryptofolioContext _context;

        private TestData Data => _factory.Data;

        public CurrencyTickerDataRequestHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<CurrencyTickerDataRequestHandler>();
            _fixerClient = _scope.ServiceProvider.GetRequiredService<FixerClient>();
            _context = _scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            factory.PurgeData();
        }

        public void Dispose() => _scope.Dispose();

        [Fact]
        public async Task Handle_Test()
        {
            // Setup
            _context.Currencies.AddRange(Data.USD, Data.EUR);
            _context.SaveChanges();
            var request = new CurrencyTickerDataRequest
            {
                TraceIdentifier = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                Currency = Data.USD.Code,
                VsCurrencies = new[] { Data.EUR.Code }
            };

            // Act
            await _handler.Handle(request, CancellationToken.None, null);

            // Assert
            var rates = await _fixerClient.GetLatestRatesAsync(Data.USD.Code, new[] { Data.EUR.Code }, CancellationToken.None);
            var ticker = _context.CurrencyTickers.Single();
            ticker.Currency.Id.Should().Be(Data.USD.Id);
            ticker.VsCurrency.Id.Should().Be(Data.EUR.Id);
            ticker.Timestamp.Should().Be(rates.Timestamp);
            ticker.Value.Should().Be(rates.Rates[Data.EUR.Code.ToUpperInvariant()]);
        }

        [Fact]
        public void Handle_RequestCancelled_Test()
        {
            // Setup
            var request = new CurrencyTickerDataRequest
            {
                TraceIdentifier = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                Currency = Data.USD.Code,
                VsCurrencies = new[] { Data.EUR.Code }
            };
            var cancellationToken = new CancellationToken(true);

            // Act
            _handler.Awaiting(h => h.Handle(request, cancellationToken, null)).Should().Throw<OperationCanceledException>();
        }
    }
}
