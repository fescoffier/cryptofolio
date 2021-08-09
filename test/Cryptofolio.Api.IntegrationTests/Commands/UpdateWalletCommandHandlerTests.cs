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
    public class UpdateWalletCommandHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly TestData _data;
        private readonly IServiceScope _scope;
        private readonly UpdateWalletCommandHandler _handler;
        private readonly CryptofolioContext _context;
        private readonly Mock<ISystemClock> _systemClockMock;
        private readonly Mock<IEventDispatcher> _dispatcherMock;

        public UpdateWalletCommandHandlerTests(WebApplicationFactory factory)
        {
            _data = factory.Data;
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<UpdateWalletCommandHandler>();
            _context = _scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            _systemClockMock = _scope.ServiceProvider.GetRequiredService<Mock<ISystemClock>>();
            _dispatcherMock = _scope.ServiceProvider.GetRequiredService<Mock<IEventDispatcher>>();
        }

        [Fact]
        public async Task Handle_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            _context.Wallets.Add(_data.Wallet1);
            _context.SaveChanges();
            _context.ChangeTracker.Clear();
            var command = new UpdateWalletCommand
            {
                RequestContext = new(null, _data.UserId),
                Id = _data.Wallet1.Id,
                Name = _data.Wallet1.Name + " updated",
                Description = _data.Wallet1.Description
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Succeeded.Should().BeTrue();
            var wallet = _context.Wallets.Single(w => w.Id == _data.Wallet1.Id);
            wallet.Name.Should().Be(command.Name);
            wallet.Description.Should().Be(command.Description);
            _dispatcherMock.Verify(m => m.DispatchAsync(It.Is<WalletUpdatedEvent>(w => w.Date == utcNow)), Times.Once());
        }

        [Fact]
        public async Task Handle_Failed_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            var command = new UpdateWalletCommand
            {
                RequestContext = new(null, _data.UserId),
                Id = _data.Wallet1.Id,
                Name = _data.Wallet1.Name + " updated",
                Description = _data.Wallet1.Description
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().HaveCount(1).And.Contain(CommandConstants.Wallet.Errors.UpdateError);
            _dispatcherMock.Verify(m => m.DispatchAsync(It.Is<WalletUpdatedEvent>(w => w.Date == utcNow)), Times.Never());
        }
    }
}
