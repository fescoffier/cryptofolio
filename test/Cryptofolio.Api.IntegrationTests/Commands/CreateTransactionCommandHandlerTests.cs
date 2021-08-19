using Cryptofolio.Api.Commands;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Entities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Api.IntegrationTests.Commands
{
    public class CreateTransactionCommandHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly TestData _data;
        private readonly IServiceScope _scope;
        private readonly CreateTransactionCommandHandler _handler;
        private readonly CryptofolioContext _context;
        private readonly Mock<ISystemClock> _systemClockMock;
        private readonly Mock<IEventDispatcher> _dispatcherMock;

        public CreateTransactionCommandHandlerTests(WebApplicationFactory factory)
        {
            _data = factory.Data;
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<CreateTransactionCommandHandler>();
            _context = _scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            _systemClockMock = _scope.ServiceProvider.GetRequiredService<Mock<ISystemClock>>();
            _dispatcherMock = _scope.ServiceProvider.GetRequiredService<Mock<IEventDispatcher>>();

            factory.PurgeData();
        }

        [Fact]
        public async Task Handle_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            _context.Wallets.Add(_data.Transaction1.Wallet);
            _context.Assets.Add(_data.Transaction1.Asset);
            _context.Exchanges.Add(_data.Transaction1.Exchange);
            _context.SaveChanges();
            var command = new CreateTransactionCommand
            {
                RequestContext = new(null, _data.UserId),
                Type = CommandConstants.Transaction.Types.Buy,
                Date = _data.Transaction1.Date,
                WalletId = _data.Transaction1.Wallet.Id,
                AssetId = _data.Transaction1.Asset.Id,
                ExchangeId = _data.Transaction1.Exchange.Id,
                Currency = _data.Transaction1.Currency,
                Price = _data.Transaction1.Price,
                Qty = _data.Transaction1.Qty,
                Note = _data.Transaction1.Note
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(_data.Transaction1, options => options.Excluding(m => m.Id));
            _context.Transactions.Single(t => t.Id == result.Data.Id).Should().BeEquivalentTo(result.Data);
            _dispatcherMock.Verify(m => m.DispatchAsync(It.Is<TransactionCreatedEvent>(w => w.Date == utcNow)), Times.Once());
        }

        [Fact]
        public async Task Handle_InvalidWallet_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            _context.Assets.Add(_data.Transaction1.Asset);
            _context.Exchanges.Add(_data.Transaction1.Exchange);
            _context.SaveChanges();
            var command = new CreateTransactionCommand
            {
                RequestContext = new(null, _data.UserId),
                Type = CommandConstants.Transaction.Types.Buy,
                Date = _data.Transaction1.Date,
                WalletId = _data.Transaction1.Wallet.Id,
                AssetId = _data.Transaction1.Asset.Id,
                ExchangeId = _data.Transaction1.Exchange.Id,
                Currency = _data.Transaction1.Currency,
                Price = _data.Transaction1.Price,
                Qty = _data.Transaction1.Qty,
                Note = _data.Transaction1.Note
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Errors.Should().HaveCount(1).And.Contain(CommandConstants.Transaction.Errors.CreateInvalidWalletError);
            _dispatcherMock.Verify(m => m.DispatchAsync(It.Is<TransactionCreatedEvent>(w => w.Date == utcNow)), Times.Never());
        }

        [Fact]
        public async Task Handle_Failed_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            var command = new CreateTransactionCommand
            {
                RequestContext = new(null, _data.UserId)
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Errors.Should().HaveCount(1).And.Contain(CommandConstants.Transaction.Errors.CreateError);
            _dispatcherMock.Verify(m => m.DispatchAsync(It.Is<TransactionCreatedEvent>(w => w.Date == utcNow)), Times.Never());
        }
    }
}
