using Cryptofolio.Api.Commands;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Entities;
using Cryptofolio.Infrastructure.TestsCommon;
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
    public class DeleteWalletCommandHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly DeleteWalletCommandHandler _handler;
        private readonly CryptofolioContext _context;
        private readonly Mock<ISystemClock> _systemClockMock;
        private readonly Mock<IEventDispatcher> _dispatcherMock;

        private TestData Data => _factory.Data;

        public DeleteWalletCommandHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<DeleteWalletCommandHandler>();
            _context = _scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            _systemClockMock = _scope.ServiceProvider.GetRequiredService<Mock<ISystemClock>>();
            _dispatcherMock = _scope.ServiceProvider.GetRequiredService<Mock<IEventDispatcher>>();
            _factory.PurgeData();
        }

        [Fact]
        public async Task Handle_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            _context.Wallets.Add(Data.Wallet2);
            _context.SaveChanges();
            _context.ChangeTracker.Clear();
            var command = new DeleteWalletCommand
            {
                RequestContext = new(null, Data.UserId),
                Id = Data.Wallet2.Id
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Succeeded.Should().BeTrue();
            _context.Wallets.SingleOrDefault(w => w.Id == Data.Wallet2.Id).Should().BeNull();
            _dispatcherMock.Verify(m => m.DispatchAsync(It.Is<WalletDeletedEvent>(w => w.Date == utcNow)), Times.Once());
        }

        [Fact]
        public async Task Handle_Selected_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            _context.Wallets.Add(Data.Wallet1);
            _context.SaveChanges();
            _context.ChangeTracker.Clear();
            var command = new DeleteWalletCommand
            {
                RequestContext = new(null, Data.UserId),
                Id = Data.Wallet1.Id
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().HaveCount(1).And.Contain(CommandConstants.Wallet.Errors.DeleteSelectedError);
            _context.Wallets.SingleOrDefault(w => w.Id == Data.Wallet1.Id).Should().NotBeNull();
            _dispatcherMock.Verify(m => m.DispatchAsync(It.Is<WalletDeletedEvent>(w => w.Date == utcNow)), Times.Never());
        }

        [Fact]
        public async Task Handle_Failed_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            _context.Wallets.Add(Data.Wallet3);
            _context.SaveChanges();
            _context.ChangeTracker.Clear();
            var command = new DeleteWalletCommand
            {
                RequestContext = new(null, Data.UserId),
                Id = ""
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().HaveCount(1).And.Contain(CommandConstants.Wallet.Errors.DeleteError);
            _context.Wallets.SingleOrDefault(w => w.Id == Data.Wallet3.Id).Should().NotBeNull();
            _dispatcherMock.Verify(m => m.DispatchAsync(It.Is<WalletDeletedEvent>(w => w.Date == utcNow)), Times.Never());
        }
    }
}
