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
    public class CreateWalletCommandHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly CreateWalletCommandHandler _handler;
        private readonly CryptofolioContext _context;
        private readonly Mock<ISystemClock> _systemClockMock;
        private readonly Mock<IEventDispatcher> _dispatcherMock;

        private TestData Data => _factory.Data;

        public CreateWalletCommandHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<CreateWalletCommandHandler>();
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
            var command = new CreateWalletCommand
            {
                RequestContext = new(null, Data.UserId),
                Name = Data.Wallet1.Name,
                Description = Data.Wallet1.Description,
                CurrencyId = Data.Wallet1.Currency.Id
            };
            var cancellationToken = CancellationToken.None;
            _context.Currencies.Add(Data.USD);
            _context.SaveChanges();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(Data.Wallet1, options => options.Excluding(m => m.Id).Excluding(m => m.Selected));
            _context.Wallets
                .Include(w => w.Currency)
                .Single(w => w.Id == result.Data.Id)
                .Should()
                .BeEquivalentTo(result.Data, options => options.Excluding(m => m.Selected));
            _dispatcherMock.Verify(m => m.DispatchAsync(It.Is<WalletCreatedEvent>(w => w.Date == utcNow)), Times.Once());
        }

        [Fact]
        public async Task Handle_Failed_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            var command = new CreateWalletCommand
            {
                RequestContext = new(null, Data.UserId)
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Errors.Should().HaveCount(1).And.Contain(CommandConstants.Wallet.Errors.CreateError);
            _dispatcherMock.Verify(m => m.DispatchAsync(It.Is<WalletCreatedEvent>(w => w.Date == utcNow)), Times.Never());
        }
    }
}
