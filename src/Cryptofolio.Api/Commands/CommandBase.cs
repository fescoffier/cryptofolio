using IdentityModel;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Cryptofolio.Api.Commands
{
    /// <summary>
    /// Models a command.
    /// </summary>
    public abstract class CommandBase
    {
        /// <summary>
        /// The request id.
        /// </summary>
        public string RequestId { get; private set; }

        /// <summary>
        /// The user id.
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// Ensure the traceablity by setting the <see cref="RequestId"/> & <see cref="UserId"/> properties from the current request.
        /// </summary>
        /// <param name="httpContext"></param>
        public void EnsureTraceability(HttpContext httpContext)
        {
            if (httpContext.Request.Headers.TryGetValue("x-requestid", out var values))
            {
                RequestId = values.ToString();
            }
            UserId = httpContext.User.FindFirstValue(JwtClaimTypes.Subject);
        }
    }
}
