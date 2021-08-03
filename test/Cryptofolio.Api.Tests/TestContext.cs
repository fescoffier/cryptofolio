using IdentityModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace Cryptofolio.Api.Tests
{
    public class TestContext
    {
        public static TestContext Instance { get; } = new();

        public string RequestId { get; }

        public string UserId { get; }

        public ClaimsPrincipal User { get; }

        public HttpContext HttpContext { get; }

        public RequestContext RequestContext { get; }

        private TestContext()
        {
            RequestId = Guid.NewGuid().ToString();
            UserId = Guid.NewGuid().ToString();
            User = new(new ClaimsIdentity(new[]
            {
                new Claim(JwtClaimTypes.Subject, UserId)
            }));
            HttpContext = new DefaultHttpContext
            {
                User = User
            };
            RequestContext = new(RequestId, UserId);
        }
    }
}
