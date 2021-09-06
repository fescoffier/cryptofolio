using Cryptofolio.Handlers.Job.Transactions;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Entities;
using Cryptofolio.Infrastructure.TestsCommon;
using FluentAssertions;
using MediatR;
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
        private readonly CryptofolioContext _context;

        private TestData Data => _factory.Data;

        public TransactionEventHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<TransactionEventHandler>();
            _context = _scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            factory.PurgeData();
        }

        [Fact]
        public async Task Handle_TransactionCreatedEvent_Test()
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

            // Act
            var result = await _handler.Handle(@event, cancellationToken, null);

            // Assert
            result.Should().Be(Unit.Value);
            var holding = _context.Holdings.Single(h => h.Wallet.Id == Data.Transaction1.Wallet.Id && h.Asset.Id == Data.Transaction1.Asset.Id);
            holding.Qty.Should().Be(Data.Holding1.Qty);
            holding.InitialValue.Should().Be(Data.Holding1.InitialValue);
        }

        [Fact]
        public async Task Handle_TransactionUpdatedEvent_Test()
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

            // Act
            var result = await _handler.Handle(@event, cancellationToken, null);

            // Assert
            result.Should().Be(Unit.Value);
            var holding = _context.Holdings.Single(h => h.Wallet.Id == Data.Transaction2.Wallet.Id && h.Asset.Id == Data.Transaction2.Asset.Id);
            holding.Qty.Should().Be(0);
            holding.InitialValue.Should().Be(0);
        }

        [Fact]
        public async Task Handle_TransactionDeletedEvent_WithoutHoldingRemoval_Test()
        {
            // Setup
            _context.Transactions.Add(Data.Transaction1);
            _context.SaveChanges();
            var @event = new TransactionCreatedEvent
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                UserId = Guid.NewGuid().ToString(),
                Transaction = Data.Transaction2
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(@event, cancellationToken, null);

            // Assert
            result.Should().Be(Unit.Value);
            var holding = _context.Holdings.Single(h => h.Wallet.Id == Data.Transaction2.Wallet.Id && h.Asset.Id == Data.Transaction2.Asset.Id);
            holding.Qty.Should().Be(Data.Transaction1.Qty);
            holding.InitialValue.Should().Be(Data.Transaction1.InitialValue);
        }

        [Fact]
        public async Task Handle_TransactionDeletedEvent_WithHoldingRemoval_Test()
        {
            // Setup
            var @event = new TransactionCreatedEvent
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                UserId = Guid.NewGuid().ToString(),
                Transaction = Data.Transaction2
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(@event, cancellationToken, null);

            // Assert
            result.Should().Be(Unit.Value);
            _context.Holdings
                .SingleOrDefault(h => h.Wallet.Id == Data.Transaction2.Wallet.Id && h.Asset.Id == Data.Transaction2.Asset.Id)
                .Should()
                .BeNull();
        }
    }
}
