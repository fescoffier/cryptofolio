using Confluent.Kafka;
using Cryptofolio.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Cryptofolio.Infrastructure
{
    /// <summary>
    /// Provices extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class KafkaServiceCollectionExtensions
    {
        /// <summary>
        /// Add a <see cref="KafkaProducerWrapper{string, TMessage}"/> with its configuration in the service collection.
        /// </summary>
        /// <typeparam name="TMessage">The type of message that will be produced by the producer.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">The configuration method.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddProducer<TMessage>(this IServiceCollection services, Action<KafkaProducerOptions<TMessage>> configureOptions)
        {
            services.Configure(configureOptions);
            services.PostConfigure<KafkaProducerOptions<TMessage>>(options =>
            {
                options.ValueSerilializerOptions ??= new();
                options.ValueSerilializerOptions.PropertyNameCaseInsensitive = true;
                options.ValueSerilializerOptions.Converters.Add(new IEventJsonConverter());
            });
            services.AddSingleton(p =>
            {
                var optionsMonitor = p.GetRequiredService<IOptionsMonitor<KafkaProducerOptions<TMessage>>>();
                var producer = new ProducerBuilder<string, TMessage>(optionsMonitor.CurrentValue.Config)
                    .SetValueSerializer(new KafkaMessageSerializer<TMessage>(optionsMonitor))
                    .Build();
                return new KafkaProducerWrapper<string, TMessage>(producer, optionsMonitor);
            });
            return services;
        }

        /// <summary>
        /// Add a <see cref="KafkaConsumerWrapper{string, TMessage}"/> with its configuration in the service collection.
        /// </summary>
        /// <typeparam name="TMessage">The type of message that will be consumed by the consumer.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">The configuration method.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddConsumer<TMessage>(this IServiceCollection services, Action<KafkaConsumerOptions<TMessage>> configureOptions)
        {
            services.Configure(configureOptions);
            services.PostConfigure<KafkaConsumerOptions<TMessage>>(options =>
            {
                options.ValueSerilializerOptions ??= new();
                options.ValueSerilializerOptions.PropertyNameCaseInsensitive = true;
                options.ValueSerilializerOptions.Converters.Add(new IEventJsonConverter());
            });
            services.AddSingleton(p =>
            {
                var optionsMonitor = p.GetRequiredService<IOptionsMonitor<KafkaConsumerOptions<TMessage>>>();
                var producer = new ConsumerBuilder<string, TMessage>(optionsMonitor.CurrentValue.Config)
                    .SetValueDeserializer(new KafkaMessageSerializer<TMessage>(optionsMonitor))
                    .Build();
                return new KafkaConsumerWrapper<string, TMessage>(producer, optionsMonitor);
            });
            services.AddHostedService<KafkaMessageHandler<TMessage>>();
            return services;
        }
    }
}
