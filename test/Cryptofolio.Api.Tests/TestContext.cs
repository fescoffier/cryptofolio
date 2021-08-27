using Cryptofolio.Infrastructure.TestsCommon;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace Cryptofolio.Api.Tests
{
    public class TestContext
    {
        public static TestContext Instance { get; } = new();

        public TestData Data { get; }

        public ClaimsPrincipal User { get; }

        public HttpContext HttpContext { get; }

        public RequestContext RequestContext { get; }

        private TestContext()
        {
            Data = new();
            User = new(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Data.UserId)
            }));
            HttpContext = new DefaultHttpContext
            {
                User = User
            };
            RequestContext = new(Guid.NewGuid().ToString(), Data.UserId);
        }
    }
}
