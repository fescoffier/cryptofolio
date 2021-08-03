using Cryptofolio.Api.Commands;
using Cryptofolio.Infrastructure;
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
    public class CreateWalletCommandHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly TestData _data = new();
        private readonly IServiceScope _scope;
        private readonly CreateWalletCommandHandler _handler;
        private readonly CryptofolioContext _context;
        private readonly Mock<ISystemClock> _systemClockMock;
        private readonly Mock<IEventDispatcher> _dispatcherMock;

        public CreateWalletCommandHandlerTests(WebApplicationFactory factory)
        {
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
                RequestContext = new(null, _data.UserId1),
                Name = _data.Wallet1.Name,
                Description = _data.Wallet1.Description
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(_data.Wallet1, options => options.Excluding(m => m.Id));
            _context.Wallets.Single(w => w.Id == result.Data.Id).Should().BeEquivalentTo(result.Data);
        }
    }
}
