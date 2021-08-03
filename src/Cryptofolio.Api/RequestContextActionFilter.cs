using Cryptofolio.Api.Commands;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
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
                command.RequestContext = context.HttpContext.RequestServices.GetRequiredService<RequestContext>();
            }
        }

        /// <inheritdoc/>
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
