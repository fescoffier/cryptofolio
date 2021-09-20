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
        private readonly KafkaProducerWrapper<string, ComputeWalletBalanceResponse> _producerWrapper;
        private readonly KafkaConsumerWrapper<string, ComputeWalletBalanceResponse> _consumerWrapper;
        private readonly CryptofolioContext _context;
        private readonly AssetTickerCache _tickerCache;

        private TestData Data => _factory.Data;

        public ComputeWalletBalanceRequestHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<ComputeWalletBalanceRequestHandler>();
            _producerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaProducerWrapper<string, ComputeWalletBalanceResponse>>();
            _consumerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaConsumerWrapper<string, ComputeWalletBalanceResponse>>();
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
                    Value = Data.BTC_USD_Ticker.Value * 1.2m
                });
            var request = new ComputeWalletBalanceRequest
            {
                WalletId = Data.Wallet1.Id
            };
            var cancellationToken = CancellationToken.None;
            _producerWrapper.Options.Topic = _consumerWrapper.Options.Topic = Guid.NewGuid().ToString();

            // Act
            await _handler.Handle(request, cancellationToken);

            // Assert
            Data.Transaction1.CurrentValue.Should().Be(1_200_000);
            Data.Transaction1.Change.Should().Be(20);
            Data.Transaction2.CurrentValue.Should().Be(240_000);
            Data.Transaction2.Change.Should().Be(-5.948742064425111685868798500m);
            Data.Holding1.CurrentValue.Should().Be(960_000);
            Data.Holding1.Change.Should().Be(20m);
            Data.Wallet1.CurrentValue.Should().Be(960_000);
            Data.Wallet1.Change.Should().Be(20m);

            _consumerWrapper.Consumer.Subscribe(_consumerWrapper.Options.Topic);
            var cr = _consumerWrapper.Consumer.Consume();
            _consumerWrapper.Consumer.Unsubscribe();
            cr.Message.Value.WalletId.Should().Be(Data.Wallet1.Id);
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
                    Value = Data.BTC_USD_Ticker.Value * 0.9856m
                },
                new Ticker
                {
                    Pair = new(Data.BTC.Symbol, Data.EUR.Code),
                    Timestamp = DateTimeOffset.UtcNow,
                    Value = Data.BTC_EUR_Ticker.Value * 0.9856m
                });
            var request = new ComputeWalletBalanceRequest
            {
                WalletId = Data.Wallet2.Id
            };
            var cancellationToken = CancellationToken.None;
            _producerWrapper.Options.Topic = _consumerWrapper.Options.Topic = Guid.NewGuid().ToString();

            // Act
            await _handler.Handle(request, cancellationToken);

            // Assert
            Data.Transaction3.CurrentValue.Should().Be(7_884_800);
            Data.Transaction3.Change.Should().Be(-1.44m);
            Data.Holding2.CurrentValue.Should().Be(9_856_000);
            Data.Holding2.Change.Should().Be(2.6666666666666666666666666700m);
            Data.Wallet2.CurrentValue.Should().Be(9_856_000);
            Data.Wallet2.Change.Should().Be(2.6666666666666666666666666700m);

            _consumerWrapper.Consumer.Subscribe(_consumerWrapper.Options.Topic);
            var cr = _consumerWrapper.Consumer.Consume();
            _consumerWrapper.Consumer.Unsubscribe();
            cr.Message.Value.WalletId.Should().Be(Data.Wallet2.Id);
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
                    Value = Data.ETH_EUR_Ticker.Value * 1.3759m
                });
            var request = new ComputeWalletBalanceRequest
            {
                WalletId = Data.Wallet3.Id
            };
            var cancellationToken = CancellationToken.None;
            _producerWrapper.Options.Topic = _consumerWrapper.Options.Topic = Guid.NewGuid().ToString();

            // Act
            await _handler.Handle(request, cancellationToken);

            // Assert
            Data.Transaction4.CurrentValue.Should().Be(1100720);
            Data.Transaction4.Change.Should().Be(37.59m);
            Data.Holding3.CurrentValue.Should().Be(1100720);
            Data.Holding3.Change.Should().Be(37.59m);
            Data.Wallet3.CurrentValue.Should().Be(1100720);
            Data.Wallet3.Change.Should().Be(37.59m);

            _consumerWrapper.Consumer.Subscribe(_consumerWrapper.Options.Topic);
            var cr = _consumerWrapper.Consumer.Consume();
            _consumerWrapper.Consumer.Unsubscribe();
            cr.Message.Value.WalletId.Should().Be(Data.Wallet3.Id);
        }
    }
}
