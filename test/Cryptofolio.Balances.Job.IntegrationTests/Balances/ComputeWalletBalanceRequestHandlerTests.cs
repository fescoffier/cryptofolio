using Cryptofolio.Balances.Job.Balances;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Balances;
using Cryptofolio.Infrastructure.Caching;
using Cryptofolio.Infrastructure.TestsCommon;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Balances.Job.IntegrationTests.Balances
{
    public class ComputeWalletBalanceRequestHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly ComputeWalletBalanceRequestHandler _handler;
        private readonly CryptofolioContext _context;
        private readonly AssetTickerCache _tickerCache;

        private TestData Data => _factory.Data;

        public ComputeWalletBalanceRequestHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<ComputeWalletBalanceRequestHandler>();
            _context = _scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            _tickerCache = _scope.ServiceProvider.GetRequiredService<AssetTickerCache>();
            factory.PurgeData();
        }

        [Fact]
        public async Task Handle_Wallet1_Test()
        {
            // Setup
            _context.Wallets.Add(Data.Wallet1);
            _context.Holdings.AddRange(Data.Holding1);
            _context.Transactions.AddRange(Data.Transaction1, Data.Transaction2);
            _context.SaveChanges();
            await _tickerCache.StoreTickersAsync(
                new Ticker
                {
                    Pair = new(Data.BTC.Symbol, Data.USD.Code),
                    Timestamp = DateTimeOffset.UtcNow,
                    Value = 3000
                });
            var request = new ComputeWalletBalanceRequest
            {
                WalletId = Data.Wallet1.Id
            };
            var cancellationToken = CancellationToken.None;

            // Act
            await _handler.Handle(request, cancellationToken);

            // Assert
            Data.Transaction1.CurrentValue.Should().Be(30_000);
            Data.Transaction1.Change.Should().Be(20);
            Data.Transaction2.CurrentValue.Should().Be(6_000);
            Data.Transaction2.Change.Should().Be(9.090909090909090909090909090m);
            Data.Holding1.CurrentValue.Should().Be(24_000);
            Data.Holding1.Change.Should().Be(23.076923076923076923076923080m);
            Data.Wallet1.CurrentValue.Should().Be(24_000);
            Data.Wallet1.Change.Should().Be(23.076923076923076923076923080m);
        }

        [Fact]
        public async Task Handle_Wallet2_Test()
        {
            // Setup
            _context.Wallets.Add(Data.Wallet2);
            _context.Holdings.AddRange(Data.Holding2);
            _context.Transactions.AddRange(Data.Transaction3);
            _context.SaveChanges();
            await _tickerCache.StoreTickersAsync(
                new Ticker
                {
                    Pair = new(Data.BTC.Symbol, Data.USD.Code),
                    Timestamp = DateTimeOffset.UtcNow,
                    Value = 300
                },
                new Ticker
                {
                    Pair = new(Data.BTC.Symbol, Data.EUR.Code),
                    Timestamp = DateTimeOffset.UtcNow,
                    Value = 250
                });
            var request = new ComputeWalletBalanceRequest
            {
                WalletId = Data.Wallet2.Id
            };
            var cancellationToken = CancellationToken.None;

            // Act
            await _handler.Handle(request, cancellationToken);

            // Assert
            Data.Transaction3.CurrentValue.Should().Be(25_000);
            Data.Transaction3.Change.Should().Be(66.666666666666666666666666670m);
            Data.Holding2.CurrentValue.Should().Be(30_000);
            Data.Holding2.Change.Should().Be(66.666666666666666666666666670m);
            Data.Wallet2.CurrentValue.Should().Be(30_000);
            Data.Wallet2.Change.Should().Be(66.666666666666666666666666670m);
        }

        [Fact]
        public async Task Handle_Wallet3_Test()
        {
            // Setup
            _context.Wallets.Add(Data.Wallet3);
            _context.Holdings.AddRange(Data.Holding3);
            _context.Transactions.AddRange(Data.Transaction4);
            _context.SaveChanges();
            await _tickerCache.StoreTickersAsync(
                new Ticker
                {
                    Pair = new(Data.ETH.Symbol, Data.EUR.Code),
                    Timestamp = DateTimeOffset.UtcNow,
                    Value = 21000m
                });
            var request = new ComputeWalletBalanceRequest
            {
                WalletId = Data.Wallet3.Id
            };
            var cancellationToken = CancellationToken.None;

            // Act
            await _handler.Handle(request, cancellationToken);

            // Assert
            Data.Transaction4.CurrentValue.Should().Be(1_050_000);
            Data.Transaction4.Change.Should().Be(16.666666666666666666666666670m);
            Data.Holding3.CurrentValue.Should().Be(1_050_000);
            Data.Holding3.Change.Should().Be(16.666666666666666666666666670m);
            Data.Wallet3.CurrentValue.Should().Be(1_050_000);
            Data.Wallet3.Change.Should().Be(16.666666666666666666666666670m);
        }
    }
}
