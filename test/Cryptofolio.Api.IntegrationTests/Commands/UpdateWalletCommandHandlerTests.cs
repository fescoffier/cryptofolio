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
    public class UpdateWalletCommandHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly UpdateWalletCommandHandler _handler;
        private readonly CryptofolioContext _context;
        private readonly Mock<ISystemClock> _systemClockMock;
        private readonly Mock<IEventDispatcher> _dispatcherMock;

        private TestData Data => _factory.Data;

        public UpdateWalletCommandHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
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
            _context.Wallets.Add(Data.Wallet1);
            _context.SaveChanges();
            _context.ChangeTracker.Clear();
            var command = new UpdateWalletCommand
            {
                RequestContext = new(null, Data.UserId),
                Id = Data.Wallet1.Id,
                Name = Data.Wallet1.Name + " updated",
                Description = Data.Wallet1.Description,
                CurrencyId = Data.Wallet1.Currency.Id
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Succeeded.Should().BeTrue();
            var wallet = _context.Wallets.Single(w => w.Id == Data.Wallet1.Id);
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
                RequestContext = new(null, Data.UserId),
                Id = Data.Wallet1.Id,
                Name = Data.Wallet1.Name + " updated",
                Description = Data.Wallet1.Description
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
