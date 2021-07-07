using Confluent.Kafka;
using Cronos;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Collector.Job.Data
{
    /// <summary>
    /// Provides a base scheduler for <see cref="TRequest"/>.
    /// </summary>
    /// <typeparam name="TRequest">The data request type.</typeparam>
    public abstract class DataRequestSchedulerBase<TRequest> : BackgroundService where TRequest : DataRequest
    {
        private string _cron;
        private DateTime? _nextExecution;
        private readonly IServiceProvider _provider;

        /// <summary>
        /// The producer wrapper.
        /// </summary>
        protected KafkaProducerWrapper<string, TRequest> ProducerWrapper { get; }

        /// <summary>
        /// The producer.
        /// </summary>
        protected IProducer<string, TRequest> Producer => ProducerWrapper.Producer;

        /// <summary>
        /// The producer's options.
        /// </summary>
        protected KafkaOptions<TRequest> ProducerOptions => ProducerWrapper.Options;

        /// <summary>
        /// The options monitor.
        /// </summary>
        protected IOptionsMonitor<DataRequestSchedulerOptions> OptionsMonitor { get; }

        /// <summary>
        /// The options.
        /// </summary>
        protected DataRequestSchedulerOptions Options => OptionsMonitor.CurrentValue;

        /// <summary>
        /// The system clock.
        /// </summary>
        protected ISystemClock SystemClock { get; }

        /// <summary>
        /// The logger.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Creates a new instance of <see cref="DataRequestSchedulerBase{TRequest}"/>.
        /// </summary>
        /// <param name="provider">The service provider.</param>
        /// <param name="producerWrapper">The producer wrapper.</param>
        /// <param name="optionsMonitor">The options monitor.</param>
        /// <param name="systemClock">The system clock.</param>
        /// <param name="logger">The logger.</param>
        protected DataRequestSchedulerBase(
            IServiceProvider provider,
            KafkaProducerWrapper<string, TRequest> producerWrapper,
            IOptionsMonitor<DataRequestSchedulerOptions> optionsMonitor,
            ISystemClock systemClock,
            ILogger logger
        )
        {
            _provider = provider;
            ProducerWrapper = producerWrapper;
            OptionsMonitor = optionsMonitor;
            OptionsMonitor.OnChange((_, __) => ComputeNextExecution());
            SystemClock = systemClock;
            Logger = logger;
        }

        private void ComputeNextExecution()
        {
            _cron = Options.Schedules[typeof(TRequest).FullName];
            _nextExecution = CronExpression.Parse(_cron).GetNextOccurrence(SystemClock.UtcNow.UtcDateTime);
            Logger.LogTrace("Next execution scheduled at {0} with cron: {1}", _nextExecution, _cron);
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ComputeNextExecution();
                if (_nextExecution == null)
                {
                    Logger.LogWarning("The {0} data request's schedule models an invalid cron: {1}", typeof(TRequest).FullName, _cron);
                    await Task.Delay(Options.InvalidCronCheckIntervalMs, stoppingToken);
                }
                else
                {
                    while (_nextExecution > SystemClock.UtcNow.UtcDateTime)
                    {
                        await Task.Delay(Options.CronCheckIntervalMs, stoppingToken);
                    }

                    using var scope = _provider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
                    var messages = await PrepareMessages(context, stoppingToken);
                    foreach (var message in messages)
                    {
                        Producer.Produce(ProducerOptions.Topic, message, pr =>
                        {
                            if (pr.Status != PersistenceStatus.NotPersisted)
                            {
                                Logger.LogDebug(
                                    "Data request for {0} delivered to topic {1} at partition {2} and offset {3}.",
                                    typeof(TRequest).FullName,
                                    pr.Topic,
                                    pr.TopicPartitionOffset.Partition,
                                    pr.TopicPartitionOffset.Offset
                                );
                            }
                            else
                            {
                                if (Logger.IsEnabled(LogLevel.Error))
                                {
                                    Logger.LogError("Failed to deliver the data request for {0}: {1}",
                                        typeof(TRequest).FullName,
                                        JsonSerializer.Serialize(pr.Value)
                                    );
                                }
                            }
                        });
                    }
                    Producer.Flush();
                }
            }
        }

        /// <summary>
        /// Prepares the messages to be sent.
        /// </summary>
        /// <param name="context">The Db context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{IEnumerable{Message{string, TRequest}}}"/> that represents the asynchronous operation.</returns>
        protected abstract Task<IEnumerable<Message<string, TRequest>>> PrepareMessages(CryptofolioContext context, CancellationToken cancellationToken);
    }
}
