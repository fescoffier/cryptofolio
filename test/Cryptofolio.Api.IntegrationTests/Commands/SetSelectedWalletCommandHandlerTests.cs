using Cryptofolio.Api.Commands;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Entities;
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
    public class SetSelectedWalletCommandHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly TestData _data;
        private readonly IServiceScope _scope;
        private readonly SetSelectedWalletCommandHandler _handler;
        private readonly CryptofolioContext _context;
        private readonly Mock<ISystemClock> _systemClockMock;
        private readonly Mock<IEventDispatcher> _dispatcherMock;

        public SetSelectedWalletCommandHandlerTests(WebApplicationFactory factory)
        {
            _data = factory.Data;
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<SetSelectedWalletCommandHandler>();
            _context = _scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            _systemClockMock = _scope.ServiceProvider.GetRequiredService<Mock<ISystemClock>>();
            _dispatcherMock = _scope.ServiceProvider.GetRequiredService<Mock<IEventDispatcher>>();

            _context.Database.ExecuteSqlRaw("delete from \"data\".\"wallet\"");
        }

        [Fact]
        public async Task Handle_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            _context.Wallets.AddRange(_data.Wallet1, _data.Wallet2, _data.Wallet3);
            _context.SaveChanges();
            _context.ChangeTracker.Clear();
            var command = new SetSelectedWalletCommand
            {
                RequestContext = new(null, _data.UserId),
                Id = _data.Wallet2.Id
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Succeeded.Should().BeTrue();
            var wallets = _context.Wallets.ToList();
            wallets.Single(w => w.Id == _data.Wallet1.Id).Selected.Should().BeFalse();
            wallets.Single(w => w.Id == _data.Wallet2.Id).Selected.Should().BeTrue();
            wallets.Single(w => w.Id == _data.Wallet3.Id).Selected.Should().BeFalse();
            _dispatcherMock.Verify(m => m.DispatchAsync(It.Is<WalletSelectedEvent>(w => w.Date == utcNow)), Times.Once());
        }

        [Fact]
        public async Task Handle_Failed_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _context.Wallets.AddRange(_data.Wallet1, _data.Wallet2, _data.Wallet3);
            _context.SaveChanges();
            _context.ChangeTracker.Clear();
            var command = new SetSelectedWalletCommand
            {
                RequestContext = new(null, _data.UserId)
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().HaveCount(1).And.Contain(CommandConstants.Wallet.Errors.UpdateError);
            var wallets = _context.Wallets.ToList();
            wallets.Single(w => w.Id == _data.Wallet1.Id).Selected.Should().BeTrue();
            wallets.Single(w => w.Id == _data.Wallet2.Id).Selected.Should().BeFalse();
            wallets.Single(w => w.Id == _data.Wallet3.Id).Selected.Should().BeFalse();
            _dispatcherMock.Verify(m => m.DispatchAsync(It.Is<WalletSelectedEvent>(w => w.Date == utcNow)), Times.Never());
        }
    }
}
