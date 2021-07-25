using Cryptofolio.Infrastructure;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Handlers.Job.Tests
{
    public class EventTraceFinalizerTests
    {
        [Fact]
        public async Task Handle_Test()
        {
            // Setup
            var handler = new EventTraceFinalizer<FakeEvent>(new NullLogger<EventTraceFinalizer<FakeEvent>>());
            var @event = new FakeEvent();
            var cancellationToken = CancellationToken.None;
            var nextMock = new Mock<RequestHandlerDelegate<Unit>>();
            nextMock.Setup(m => m()).ReturnsAsync(Unit.Value);

            // Act
            var result = await handler.Handle(@event, cancellationToken, nextMock.Object);

            // Assert
            result.Should().Be(Unit.Value);
            nextMock.Verify(m => m(), Times.Never());
        }

        private class FakeEvent : IEvent
        {
            public string Id { get; init; }

            public DateTimeOffset Date { get; init; }

            public string UserId { get; init; }

            public string Username { get; init; }

            public string Category => "Test";
        }
    }
}
