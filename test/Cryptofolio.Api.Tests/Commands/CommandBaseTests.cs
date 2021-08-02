using Cryptofolio.Api.Commands;
using FluentAssertions;
using Xunit;

namespace Cryptofolio.Api.Tests.Commands
{
    public class CommandBaseTests
    {
        [Fact]
        public void EnsureTraceability_Test()
        {
            // Setup
            var requestId = TestContext.Instance.RequestId;
            var userId = TestContext.Instance.UserId;
            var httpContext = TestContext.Instance.HttpContext;
            var command = new FakeCommand();

            // Act
            command.EnsureTraceability(httpContext);

            // Assert
            command.RequestId.Should().Be(requestId);
            command.UserId.Should().Be(userId);
        }


        private class FakeCommand : CommandBase
        {
        }
    }
}
