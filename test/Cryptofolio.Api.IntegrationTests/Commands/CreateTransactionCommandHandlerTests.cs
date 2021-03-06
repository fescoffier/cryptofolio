using Cryptofolio.Api.Commands;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Entities;
using Cryptofolio.Infrastructure.TestsCommon;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly CreateTransactionCommandHandler _handler;
        private readonly CryptofolioContext _context;
        private readonly Mock<ISystemClock> _systemClockMock;
        private readonly Mock<IEventDispatcher> _dispatcherMock;

        private TestData Data => _factory.Data;

        public CreateTransactionCommandHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<CreateTransactionCommandHandler>();
            _context = _scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            _systemClockMock = _scope.ServiceProvider.GetRequiredService<Mock<ISystemClock>>();
            _dispatcherMock = _scope.ServiceProvider.GetRequiredService<Mock<IEventDispatcher>>();

            factory.PurgeData();
        }

        [Fact]
        public async Task Handle_Buy_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            _context.Wallets.Add(Data.Transaction1.Wallet);
            _context.Assets.Add(Data.Transaction1.Asset);
            _context.Exchanges.Add(Data.Transaction1.Exchange);
            _context.Currencies.Add(Data.Transaction1.Currency);
            _context.SaveChanges();
            var command = new CreateTransactionCommand
            {
                RequestContext = new(null, Data.UserId),
                Type = CommandConstants.Transaction.Types.Buy,
                Date = Data.Transaction1.Date,
                WalletId = Data.Transaction1.Wallet.Id,
                AssetId = Data.Transaction1.Asset.Id,
                ExchangeId = Data.Transaction1.Exchange.Id,
                CurrencyId = Data.Transaction1.Currency.Id,
                Price = Data.Transaction1.Price,
                Qty = Data.Transaction1.Qty,
                Note = Data.Transaction1.Note
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(Data.Transaction1, options => options.Excluding(m => m.Id));
            _context.Transactions
                .OfType<BuyOrSellTransaction>()
                .Include(t => t.Wallet)
                .Include(t => t.Asset)
                .Include(t => t.Exchange)
                .Include(t => t.Currency)
                .Single(t => t.Id == result.Data.Id)
                .Should()
                .BeEquivalentTo(result.Data);
            _dispatcherMock.Verify(m => m.DispatchAsync(It.Is<TransactionCreatedEvent>(w => w.Date == utcNow)), Times.Once());
        }

        [Fact]
        public async Task Handle_Buy_InvalidWallet_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            _context.Assets.Add(Data.Transaction1.Asset);
            _context.Exchanges.Add(Data.Transaction1.Exchange);
            _context.SaveChanges();
            var command = new CreateTransactionCommand
            {
                RequestContext = new(null, Data.UserId),
                Type = CommandConstants.Transaction.Types.Buy,
                Date = Data.Transaction1.Date,
                WalletId = Data.Transaction1.Wallet.Id,
                AssetId = Data.Transaction1.Asset.Id,
                ExchangeId = Data.Transaction1.Exchange.Id,
                CurrencyId = Data.Transaction1.Currency.Id,
                Price = Data.Transaction1.Price,
                Qty = Data.Transaction1.Qty,
                Note = Data.Transaction1.Note
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
        public async Task Handle_Sell_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            _context.Wallets.Add(Data.Transaction2.Wallet);
            _context.Assets.Add(Data.Transaction2.Asset);
            _context.Exchanges.Add(Data.Transaction2.Exchange);
            _context.Currencies.Add(Data.Transaction2.Currency);
            _context.SaveChanges();
            var command = new CreateTransactionCommand
            {
                RequestContext = new(null, Data.UserId),
                Type = CommandConstants.Transaction.Types.Sell,
                Date = Data.Transaction2.Date,
                WalletId = Data.Transaction2.Wallet.Id,
                AssetId = Data.Transaction2.Asset.Id,
                ExchangeId = Data.Transaction2.Exchange.Id,
                CurrencyId = Data.Transaction2.Currency.Id,
                Price = Data.Transaction2.Price,
                Qty = Data.Transaction2.Qty,
                Note = Data.Transaction2.Note
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(Data.Transaction2, options => options.Excluding(m => m.Id));
            _context.Transactions
                .OfType<BuyOrSellTransaction>()
                .Include(t => t.Wallet)
                .Include(t => t.Asset)
                .Include(t => t.Exchange)
                .Include(t => t.Currency)
                .Single(t => t.Id == result.Data.Id)
                .Should()
                .BeEquivalentTo(result.Data);
            _dispatcherMock.Verify(m => m.DispatchAsync(It.Is<TransactionCreatedEvent>(w => w.Date == utcNow)), Times.Once());
        }

        [Fact]
        public async Task Handle_Sell_InvalidWallet_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            _context.Assets.Add(Data.Transaction2.Asset);
            _context.Exchanges.Add(Data.Transaction2.Exchange);
            _context.SaveChanges();
            var command = new CreateTransactionCommand
            {
                RequestContext = new(null, Data.UserId),
                Type = CommandConstants.Transaction.Types.Buy,
                Date = Data.Transaction2.Date,
                WalletId = Data.Transaction2.Wallet.Id,
                AssetId = Data.Transaction2.Asset.Id,
                ExchangeId = Data.Transaction2.Exchange.Id,
                CurrencyId = Data.Transaction2.Currency.Id,
                Price = Data.Transaction2.Price,
                Qty = Data.Transaction2.Qty,
                Note = Data.Transaction2.Note
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
        public async Task Handle_Transfer_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            _context.Wallets.Add(Data.Transaction4.Wallet);
            _context.Assets.Add(Data.Transaction4.Asset);
            _context.AssetTickers.Add(Data.ETH_EUR_Ticker);
            _context.Exchanges.Add(Data.Transaction4.Exchange);
            _context.SaveChanges();
            var command = new CreateTransactionCommand
            {
                RequestContext = new(null, Data.UserId),
                Type = CommandConstants.Transaction.Types.Transfer,
                Date = Data.Transaction4.Date,
                WalletId = Data.Transaction4.Wallet.Id,
                AssetId = Data.Transaction4.Asset.Id,
                ExchangeId = Data.Transaction4.Exchange.Id,
                Qty = Data.Transaction4.Qty,
                Source = Data.Transaction4.Source,
                Destination = Data.Transaction4.Destination,
                Note = Data.Transaction4.Note
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(Data.Transaction4, options => options.Excluding(m => m.Id));
            _context.Transactions
                .OfType<TransferTransaction>()
                .Include(t => t.Wallet)
                .Include(t => t.Asset)
                .Include(t => t.Exchange)
                .Single(t => t.Id == result.Data.Id)
                .Should()
                .BeEquivalentTo(result.Data);
            _dispatcherMock.Verify(m => m.DispatchAsync(It.Is<TransactionCreatedEvent>(w => w.Date == utcNow)), Times.Once());
        }

        [Fact]
        public async Task Handle_Transfer_InvalidWallet_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            _context.Assets.Add(Data.Transaction4.Asset);
            _context.AssetTickers.Add(Data.ETH_EUR_Ticker);
            _context.Exchanges.Add(Data.Transaction4.Exchange);
            _context.SaveChanges();
            var command = new CreateTransactionCommand
            {
                RequestContext = new(null, Data.UserId),
                Type = CommandConstants.Transaction.Types.Transfer,
                Date = Data.Transaction4.Date,
                WalletId = Data.Transaction4.Wallet.Id,
                AssetId = Data.Transaction4.Asset.Id,
                ExchangeId = Data.Transaction4.Exchange.Id,
                Qty = Data.Transaction4.Qty,
                Source = Data.Transaction4.Source,
                Destination = Data.Transaction4.Destination,
                Note = Data.Transaction4.Note
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
            _context.Wallets.Add(Data.Wallet1);
            _context.SaveChanges();
            var command = new CreateTransactionCommand
            {
                RequestContext = new(null, Data.UserId),
                WalletId = Data.Wallet1.Id
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Errors.Should().HaveCount(1).And.Contain(CommandConstants.Transaction.Errors.CreateError);
            _dispatcherMock.Verify(m => m.DispatchAsync(It.Is<TransactionCreatedEvent>(w => w.Date == DateTimeOffset.UtcNow)), Times.Never());
        }
    }
}
