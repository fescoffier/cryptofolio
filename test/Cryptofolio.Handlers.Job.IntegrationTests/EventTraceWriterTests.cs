using Cryptofolio.Infrastructure;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using Moq;
using Nest;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Handlers.Job.IntegrationTests
{
    public class EventTraceWriterTests : IClassFixture<WebApplicationFactory>, IDisposable
    {
        private readonly IServiceScope _scope;
        private readonly EventTraceWriter<FakeEvent> _writer;
        private readonly IElasticClient _elasticClient;
        private readonly Mock<ISystemClock> _systemClockMock;

        public EventTraceWriterTests(WebApplicationFactory factory)
        {
            _scope = factory.Services.CreateScope();
            _writer = _scope.ServiceProvider.GetRequiredService<EventTraceWriter<FakeEvent>>();
            _elasticClient = _scope.ServiceProvider.GetRequiredService<IElasticClient>();
            _systemClockMock = _scope.ServiceProvider.GetRequiredService<Mock<ISystemClock>>();
        }

        public void Dispose() => _scope.Dispose();

        [Fact]
        public async Task Handle_Test()
        {
            // Setup
            var @event = new FakeEvent
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                UserId = Guid.NewGuid().ToString(),
                Username = "test",
                Property1 = "value1",
                Property2 = "value2"
            };
            var cancellationToken = CancellationToken.None;
            var nextMock = new Mock<RequestHandlerDelegate<Unit>>();
            nextMock.Setup(m => m()).ReturnsAsync(Unit.Value);

            // Act
            var result = await _writer.Handle(@event, cancellationToken, nextMock.Object);

            // Assert
            result.Should().Be(Unit.Value);
            _elasticClient.Indices.Refresh();
            var getResponse = _elasticClient.Get<IEvent>(@event.Id);
            getResponse.IsValid.Should().BeTrue();
            getResponse.Source.Should().BeEquivalentTo(@event);
            nextMock.Verify(m => m(), Times.Once());
        }

        [Fact]
        public async Task Handle_Failed_Test()
        {
            // Setup
            var @event = new FakeEvent
            {
                // Using a id with a length greated than 513 characters, makes the indexation fail on purpose.
                Id = new string(Enumerable.Repeat('A', 513).ToArray())
            };
            var cancellationToken = CancellationToken.None;
            var nextMock = new Mock<RequestHandlerDelegate<Unit>>();
            nextMock.Setup(m => m()).ReturnsAsync(Unit.Value);

            // Act
            var result = await _writer.Handle(@event, cancellationToken, nextMock.Object);

            // Assert
            result.Should().Be(Unit.Value);
            var getResponse = _elasticClient.Get<IEvent>(@event.Id);
            getResponse.IsValid.Should().BeFalse();
            getResponse.Source.Should().BeNull();
            nextMock.Verify(m => m(), Times.Never());
        }
    }
}
