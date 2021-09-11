using Cryptofolio.Infrastructure;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cryptofolio.Handlers.Job
{
    /// <summary>
    /// Provides extensions methods to <see cref="IServiceCollection"/> to register event related services.
    /// </summary>
    public static class EventServiceCollectionExtensions
    {
        /// <summary>
        /// Registers a default event handler in the service collection.
        /// </summary>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddDefaultEventHandler<TEvent>(this IServiceCollection services)
            where TEvent : class, IEvent
        {
            services.AddScoped<IPipelineBehavior<TEvent, Unit>, EventTraceWriter<TEvent>>();
            services.AddScoped<IPipelineBehavior<TEvent, Unit>, EventTraceFinalizer<TEvent>>();
            return services;
        }

        /// <summary>
        /// Registers the specified event handler in the service collection.
        /// </summary>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <typeparam name="THandler">The event handler type.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddEventHandler<TEvent, THandler>(this IServiceCollection services)
            where TEvent : class, IEvent
            where THandler : class, IPipelineBehavior<TEvent, Unit>
        {
            services.AddScoped<IPipelineBehavior<TEvent, Unit>, EventTraceWriter<TEvent>>();
            services.TryAddScoped<THandler>();
            services.AddScoped<IPipelineBehavior<TEvent, Unit>>(p => p.GetRequiredService<THandler>());
            services.AddScoped<IPipelineBehavior<TEvent, Unit>, EventTraceFinalizer<TEvent>>();
            return services;
        }
    }
}
