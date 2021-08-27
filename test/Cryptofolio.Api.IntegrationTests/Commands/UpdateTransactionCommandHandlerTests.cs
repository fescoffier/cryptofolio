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
    public class UpdateTransactionCommandHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly UpdateTransactionCommandHandler _handler;
        private readonly Mock<ISystemClock> _systemClockMock;
        private readonly Mock<IEventDispatcher> _dispatcherMock;

        private TestData Data => _factory.Data;

        public UpdateTransactionCommandHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<UpdateTransactionCommandHandler>();
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
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
                context.Exchanges.Add(Data.Exchange2);
                context.Currencies.Add(Data.EUR);
                context.Transactions.Add(Data.Transaction1);
                context.SaveChanges();
            }
            var command = new UpdateTransactionCommand
            {
                RequestContext = new(null, Data.UserId),
                Id = Data.Transaction1.Id,
                Type = CommandConstants.Transaction.Types.Buy,
                Date = Data.Transaction1.Date.AddDays(1),
                ExchangeId = Data.Exchange2.Id,
                CurrencyId = Data.EUR.Id,
                Price = 500,
                Qty = 20,
                Note = "Consectetur adipiscing elit"
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Succeeded.Should().BeTrue();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
                var transaction = context.Transactions
                    .OfType<BuyOrSellTransaction>()
                    .Include(t => t.Exchange)
                    .Include(t => t.Currency)
                    .Single(t => t.Id == Data.Transaction1.Id);
                transaction.Date.Should().BeCloseTo(command.Date, precision: 1);
                transaction.Exchange.Id.Should().Be(command.ExchangeId);
                transaction.Currency.Id.Should().Be(command.CurrencyId);
                transaction.Price.Should().Be(command.Price);
                transaction.Qty.Should().Be(command.Qty);
                transaction.Note.Should().Be(command.Note);
            }
            _dispatcherMock.Verify(m => m.DispatchAsync(It.Is<TransactionUpdatedEvent>(w => w.Date == utcNow)), Times.Once());
        }

        [Fact]
        public async Task Handle_Failed_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            var command = new UpdateTransactionCommand
            {
                RequestContext = new(null, Data.UserId)
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().HaveCount(1).And.Contain(CommandConstants.Transaction.Errors.UpdateError);
            _dispatcherMock.Verify(m => m.DispatchAsync(It.Is<TransactionUpdatedEvent>(w => w.Date == utcNow)), Times.Never());
        }
    }
}
