using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using Xunit;

namespace Cryptofolio.Api.Tests
{
    public class RequestContextTests
    {
        [Fact]
        public void FromHttpContext_Test()
        {
            // Setup
            var requestId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid().ToString();
            var httpContext = new DefaultHttpContext
            {
                Request =
                {
                    Headers =
                    {
                        { "X-RequestId", requestId }
                    }
                },
                User = new(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                }))
            };

            // Act
            var requestContext = RequestContext.FromHttpContext(httpContext);

            // Assert
            requestContext.RequestId.Should().Be(requestId);
            requestContext.UserId.Should().Be(userId);
        }

        [Fact]
        public void FromHttpContext_NoRequestId_Test()
        {
            // Setup
            var userId = Guid.NewGuid().ToString();
            var httpContext = new DefaultHttpContext
            {
                User = new(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                }))
            };

            // Act
            var requestContext = RequestContext.FromHttpContext(httpContext);

            // Assert
            requestContext.RequestId.Should().NotBeNull();
            requestContext.UserId.Should().Be(userId);
        }
    }
}
