using Cryptofolio.Infrastructure;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptofolio.Handlers.Job
{
    public static class EventServiceCollectionExtensions
    {
        public static IServiceCollection AddEventHandler<TEvent, THandler>(this IServiceCollection services)
            where TEvent : IEvent
            where THandler : class, IPipelineBehavior<TEvent, Unit>
        {
            services.AddScoped<THandler>();
            services.AddScoped<IPipelineBehavior<TEvent, Unit>>(p => p.GetRequiredService<THandler>());
            return services;
        }
    }
}
