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
    public class DeleteTransactionCommandHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly DeleteTransactionCommandHandler _handler;
        private readonly CryptofolioContext _context;
        private readonly Mock<ISystemClock> _systemClockMock;
        private readonly Mock<IEventDispatcher> _dispatcherMock;

        private TestData Data => _factory.Data;

        public DeleteTransactionCommandHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<DeleteTransactionCommandHandler>();
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
            _context.Transactions.Add(Data.Transaction1);
            _context.SaveChanges();
            var command = new DeleteTransactionCommand
            {
                RequestContext = new(null, Data.UserId),
                Id = Data.Transaction1.Id
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Succeeded.Should().BeTrue();
            _context.Transactions.SingleOrDefault(t => t.Id == Data.Transaction1.Id).Should().BeNull();
            _dispatcherMock.Verify(m => m.DispatchAsync(It.Is<TransactionDeletedEvent>(w => w.Date == utcNow)), Times.Once());
        }

        [Fact]
        public async Task Handle_Failed_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            _context.Transactions.Add(Data.Transaction1);
            _context.SaveChanges();
            var command = new DeleteTransactionCommand
            {
                RequestContext = new(null, Data.UserId),
                Id = ""
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().HaveCount(1).And.Contain(CommandConstants.Transaction.Errors.DeleteError);
            _context.Transactions.SingleOrDefault(t => t.Id == Data.Transaction1.Id).Should().NotBeNull();
            _dispatcherMock.Verify(m => m.DispatchAsync(It.Is<TransactionDeletedEvent>(w => w.Date == utcNow)), Times.Never());
        }
    }
}
