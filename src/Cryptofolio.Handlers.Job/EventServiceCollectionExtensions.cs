using Cryptofolio.Infrastructure;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Cryptofolio.Handlers.Job
{
    public static class EventServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultEventHandler<TEvent>(this IServiceCollection services)
            where TEvent : class, IEvent
        {
            services.AddScoped<IPipelineBehavior<TEvent, Unit>, EventTraceWriter<TEvent>>();
            services.AddScoped<IPipelineBehavior<TEvent, Unit>, EventTraceFinalizer<TEvent>>();
            return services;
        }

        public static IServiceCollection AddEventHandler<TEvent, THandler>(this IServiceCollection services)
            where TEvent : class, IEvent
            where THandler : class, IPipelineBehavior<TEvent, Unit>
        {
            services.AddScoped<IPipelineBehavior<TEvent, Unit>, EventTraceWriter<TEvent>>();
            services.AddScoped<THandler>();
            services.AddScoped<IPipelineBehavior<TEvent, Unit>>(p => p.GetRequiredService<THandler>());
            services.AddScoped<IPipelineBehavior<TEvent, Unit>, EventTraceFinalizer<TEvent>>();
            return services;
        }
    }
}
