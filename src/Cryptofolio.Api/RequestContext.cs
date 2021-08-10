using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace Cryptofolio.Api
{
    /// <summary>
    /// Provides contextual information of the current request.
    /// </summary>
    public class RequestContext
    {
        /// <summary>
        /// The current request id.
        /// </summary>
        public string RequestId { get; }

        /// <summary>
        /// The current request user.
        /// </summary>
        public string UserId { get; }

        /// <summary>
        /// Creates a new instance of <see cref="RequestContext"/>.
        /// </summary>
        /// <param name="requestId">The request id.</param>
        /// <param name="userId">The user ud.</param>
        public RequestContext(string requestId, string userId)
        {
            RequestId = requestId ?? Guid.NewGuid().ToString();
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        }

        /// <summary>
        /// Creates a new instance of <see cref="RequestContext"/> from the provided <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/>.</param>
        /// <returns>The <see cref="RequestContext"/>.</returns>
        public static RequestContext FromHttpContext(HttpContext httpContext)
        {
            string requestId;
            if (httpContext.Request.Headers.TryGetValue("x-requestid", out var values))
            {
                requestId = values.ToString();
            }
            else
            {
                requestId = Guid.NewGuid().ToString();
            }

            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return new(requestId, userId);
        }
    }
}
