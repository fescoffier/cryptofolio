using Cryptofolio.Api.Commands;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Cryptofolio.Api
{
    /// <summary>
    /// Provides an implementation of <see cref="IActionFilter"/> that watches for <see cref="CommandBase"/> typed arguments in <see cref="ActionExecutingContext.ActionArguments"/> to define their <see cref="RequestContext"/> instance.
    /// </summary>
    public class RequestContextActionFilter : IActionFilter
    {
        /// <inheritdoc/>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            foreach (var command in context.ActionArguments.Where(a => a.Value is CommandBase).Select(a => a.Value as CommandBase))
            {
                command.RequestContext = context.HttpContext.RequestServices.GetService<RequestContext>()
                    ?? throw new InvalidOperationException("Missing request context");
            }
        }

        /// <inheritdoc/>
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
