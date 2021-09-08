using Cryptofolio.Handlers.Job.Transactions;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Balances;
using Cryptofolio.Infrastructure.Entities;
using Cryptofolio.Infrastructure.TestsCommon;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Handlers.Job.IntegrationTests.Transactions
{
    public class TransactionEventHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly TransactionEventHandler _handler;
        private readonly KafkaProducerWrapper<string, ComputeWalletBalanceRequest> _producerWrapper;
        private readonly KafkaConsumerWrapper<string, ComputeWalletBalanceRequest> _consumerWrapper;
        private readonly CryptofolioContext _context;

        private TestData Data => _factory.Data;

        public TransactionEventHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<TransactionEventHandler>();
            _context = _scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            _producerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaProducerWrapper<string, ComputeWalletBalanceRequest>>();
            _consumerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaConsumerWrapper<string, ComputeWalletBalanceRequest>>();
            factory.PurgeData();
            factory.Data = new();
        }

        [Fact]
        public async Task Handle_Wallet1_TransactionCreatedEvent_Test()
        {
            // Setup
            _context.Transactions.AddRange(Data.Transaction1, Data.Transaction2);
            _context.SaveChanges();
            var @event = new TransactionCreatedEvent
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                UserId = Guid.NewGuid().ToString(),
                Transaction = Data.Transaction1
            };
            var cancellationToken = CancellationToken.None;
            EnsureRandomTopic();

            // Act
            var result = await _handler.Handle(@event, cancellationToken, null);

            // Assert
            result.Should().Be(Unit.Value);
            var holding = _context.Holdings
                .Include(h => h.Wallet)
                .Single(h => h.Wallet.Id == Data.Transaction1.Wallet.Id && h.Asset.Id == Data.Transaction1.Asset.Id);
            holding.Qty.Should().Be(Data.Holding1.Qty);
            holding.InitialValue.Should().Be(Data.Holding1.InitialValue);
            holding.Wallet.InitialValue.Should().Be(Data.Wallet1.InitialValue);
            EnsureRequestProduced(Data.Wallet1);
        }

        [Fact]
        public async Task Handle_Wallet1_TransactionUpdatedEvent_Test()
        {
            // Setup
            Data.Transaction2.Qty = Data.Transaction1.Qty;
            _context.Transactions.AddRange(Data.Transaction1, Data.Transaction2);
            _context.SaveChanges();
            var @event = new TransactionUpdatedEvent
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                UserId = Guid.NewGuid().ToString(),
                Transaction = Data.Transaction2
            };
            var cancellationToken = CancellationToken.None;
            EnsureRandomTopic();

            // Act
            var result = await _handler.Handle(@event, cancellationToken, null);

            // Assert
            result.Should().Be(Unit.Value);
            var holding = _context.Holdings
                .Include(h => h.Wallet)
                .Single(h => h.Wallet.Id == Data.Transaction2.Wallet.Id && h.Asset.Id == Data.Transaction2.Asset.Id);
            holding.Qty.Should().Be(0);
            holding.InitialValue.Should().Be(0);
            holding.Wallet.InitialValue.Should().Be(0);
            EnsureRequestProduced(Data.Wallet1);
        }

        [Fact]
        public async Task Handle_Wallet1_TransactionDeletedEvent_WithoutHoldingRemoval_Test()
        {
            // Setup
            _context.Transactions.Add(Data.Transaction1);
            _context.Holdings.Add(Data.Holding1);
            _context.SaveChanges();
            var @event = new TransactionDeletedEvent
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                UserId = Guid.NewGuid().ToString(),
                Transaction = Data.Transaction2
            };
            var cancellationToken = CancellationToken.None;
            EnsureRandomTopic();

            // Act
            var result = await _handler.Handle(@event, cancellationToken, null);

            // Assert
            result.Should().Be(Unit.Value);
            var holding = _context.Holdings
                .Include(h => h.Wallet)
                .Single(h => h.Wallet.Id == Data.Transaction2.Wallet.Id && h.Asset.Id == Data.Transaction2.Asset.Id);
            holding.Qty.Should().Be(Data.Transaction1.Qty);
            holding.InitialValue.Should().Be(Data.Transaction1.InitialValue);
            holding.Wallet.InitialValue.Should().Be(Data.Transaction1.InitialValue);
            EnsureRequestProduced(Data.Wallet1);
        }

        [Fact]
        public async Task Handle_Wallet1_TransactionDeletedEvent_WithHoldingRemoval_Test()
        {
            // Setup
            _context.Holdings.Add(Data.Holding1);
            _context.SaveChanges();
            var @event = new TransactionDeletedEvent
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                UserId = Guid.NewGuid().ToString(),
                Transaction = Data.Transaction2
            };
            var cancellationToken = CancellationToken.None;
            EnsureRandomTopic();

            // Act
            var result = await _handler.Handle(@event, cancellationToken, null);

            // Assert
            result.Should().Be(Unit.Value);
            _context.Holdings
                .SingleOrDefault(h => h.Wallet.Id == Data.Transaction2.Wallet.Id && h.Asset.Id == Data.Transaction2.Asset.Id)
                .Should()
                .BeNull();
            EnsureRequestProduced(Data.Wallet1);
        }

        [Fact]
        public async Task Handle_Wallet2_TransactionCreatedEvent_Test()
        {
            // Setup
            _context.Transactions.Add(Data.Transaction3);
            _context.CurrencyTickers.Add(Data.EUR_USD_Ticker);
            _context.SaveChanges();
            var @event = new TransactionCreatedEvent
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                UserId = Guid.NewGuid().ToString(),
                Transaction = Data.Transaction3
            };
            var cancellationToken = CancellationToken.None;
            EnsureRandomTopic();

            // Act
            var result = await _handler.Handle(@event, cancellationToken, null);

            // Assert
            result.Should().Be(Unit.Value);
            var holding = _context.Holdings
                .Include(h => h.Wallet)
                .Single(h => h.Wallet.Id == Data.Transaction3.Wallet.Id && h.Asset.Id == Data.Transaction3.Asset.Id);
            holding.Qty.Should().Be(Data.Holding2.Qty);
            holding.InitialValue.Should().Be(Data.Holding2.InitialValue);
            holding.Wallet.InitialValue.Should().Be(Data.Wallet2.InitialValue);
            EnsureRequestProduced(Data.Wallet2);
        }

        [Fact]
        public async Task Handle_Wallet2_TransactionUpdatedEvent_Test()
        {
            // Setup
            Data.Transaction3.Qty *= 2;
            Data.Transaction3.InitialValue *= 2;
            _context.Transactions.Add(Data.Transaction3);
            _context.CurrencyTickers.Add(Data.EUR_USD_Ticker);
            _context.SaveChanges();
            var @event = new TransactionCreatedEvent
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                UserId = Guid.NewGuid().ToString(),
                Transaction = Data.Transaction3
            };
            var cancellationToken = CancellationToken.None;
            EnsureRandomTopic();

            // Act
            var result = await _handler.Handle(@event, cancellationToken, null);

            // Assert
            result.Should().Be(Unit.Value);
            var holding = _context.Holdings
                .Include(h => h.Wallet)
                .Single(h => h.Wallet.Id == Data.Transaction3.Wallet.Id && h.Asset.Id == Data.Transaction3.Asset.Id);
            holding.Qty.Should().Be(Data.Holding2.Qty * 2);
            holding.InitialValue.Should().Be(Data.Holding2.InitialValue * 2);
            holding.Wallet.InitialValue.Should().Be(Data.Holding2.InitialValue * 2);
            EnsureRequestProduced(Data.Wallet2);
        }

        [Fact]
        public async Task Handle_Wallet3_TransactionCreatedEvent_Test()
        {
            // Setup
            _context.Transactions.Add(Data.Transaction4);
            _context.SaveChanges();
            var @event = new TransactionCreatedEvent
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                UserId = Guid.NewGuid().ToString(),
                Transaction = Data.Transaction4
            };
            var cancellationToken = CancellationToken.None;
            EnsureRandomTopic();

            // Act
            var result = await _handler.Handle(@event, cancellationToken, null);

            // Assert
            result.Should().Be(Unit.Value);
            var holding = _context.Holdings
                .Include(h => h.Wallet)
                .Single(h => h.Wallet.Id == Data.Transaction4.Wallet.Id && h.Asset.Id == Data.Transaction4.Asset.Id);
            holding.Qty.Should().Be(Data.Holding3.Qty);
            holding.InitialValue.Should().Be(Data.Holding3.InitialValue);
            holding.Wallet.InitialValue.Should().Be(Data.Wallet3.InitialValue);
            EnsureRequestProduced(Data.Wallet3);
        }

        [Fact]
        public async Task Handle_Wallet3_DestinationNotMyWallet_TransactionCreatedEvent_Test()
        {
            // Setup
            Data.Transaction1.Wallet = Data.Wallet3;
            Data.Transaction1.Asset = Data.ETH;
            Data.Transaction1.Qty = Data.Transaction4.Qty * 2;
            Data.Transaction1.Price = Data.ETH_USD_Ticker.Value;
            Data.Transaction1.InitialValue = Data.Transaction1.Qty * Data.Transaction1.Price;
            Data.Transaction4.Destination = InfrastructureConstants.Transactions.Destinations.ExternalDestination;
            _context.Transactions.AddRange(Data.Transaction1, Data.Transaction4);
            _context.CurrencyTickers.Add(Data.USD_EUR_Ticker);
            _context.SaveChanges();
            var @event = new TransactionCreatedEvent
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                UserId = Guid.NewGuid().ToString(),
                Transaction = Data.Transaction4
            };
            var cancellationToken = CancellationToken.None;
            EnsureRandomTopic();

            // Act
            var result = await _handler.Handle(@event, cancellationToken, null);

            // Assert
            result.Should().Be(Unit.Value);
            var holding = _context.Holdings
                .Include(h => h.Wallet)
                .Single(h => h.Wallet.Id == Data.Transaction4.Wallet.Id && h.Asset.Id == Data.Transaction4.Asset.Id);
            holding.Qty.Should().Be(50);
            holding.InitialValue.Should().Be(1_600_000);
            holding.Wallet.InitialValue.Should().Be(1_600_000);
            EnsureRequestProduced(Data.Wallet3);
        }

        [Fact]
        public async Task Handle_Wallet3_TransactionUpdatedEvent_Test()
        {
            // Setup
            Data.Transaction4.Qty *= 2;
            Data.Transaction4.InitialValue *= 2;
            _context.Transactions.Add(Data.Transaction4);
            _context.SaveChanges();
            var @event = new TransactionCreatedEvent
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                UserId = Guid.NewGuid().ToString(),
                Transaction = Data.Transaction4
            };
            var cancellationToken = CancellationToken.None;
            EnsureRandomTopic();

            // Act
            var result = await _handler.Handle(@event, cancellationToken, null);

            // Assert
            result.Should().Be(Unit.Value);
            var holding = _context.Holdings
                .Include(h => h.Wallet)
                .Single(h => h.Wallet.Id == Data.Transaction4.Wallet.Id && h.Asset.Id == Data.Transaction4.Asset.Id);
            holding.Qty.Should().Be(Data.Holding3.Qty * 2);
            holding.InitialValue.Should().Be(Data.Holding3.InitialValue * 2);
            holding.Wallet.InitialValue.Should().Be(Data.Holding3.InitialValue * 2);
            EnsureRequestProduced(Data.Wallet3);
        }

        private void EnsureRandomTopic() =>
            _producerWrapper.Options.Topic = _consumerWrapper.Options.Topic = Guid.NewGuid().ToString();

        private void EnsureRequestProduced(Wallet wallet)
        {
            _consumerWrapper.Consumer.Subscribe(_consumerWrapper.Options.Topic);
            var cr = _consumerWrapper.Consumer.Consume();
            _consumerWrapper.Consumer.Unsubscribe();
            cr.Message.Value.Should().BeEquivalentTo(new ComputeWalletBalanceRequest
            {
                WalletId = wallet.Id
            });
        }
    }
}
