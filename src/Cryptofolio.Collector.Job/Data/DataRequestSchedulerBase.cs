using Confluent.Kafka;
using Cronos;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        public const string SchedulesHashScheduledForField = "ScheduledFor";
        public const string SchedulesHashScheduledAtField = "ScheduledAt";

        private readonly IServiceProvider _provider;
        private readonly Random _random;

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
        /// The Redis database.
        /// </summary>
        protected IDatabase Database { get; }

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
        /// <param name="database">The Redis database.</param>
        /// <param name="systemClock">The system clock.</param>
        /// <param name="logger">The logger.</param>
        protected DataRequestSchedulerBase(
            IServiceProvider provider,
            KafkaProducerWrapper<string, TRequest> producerWrapper,
            IOptionsMonitor<DataRequestSchedulerOptions> optionsMonitor,
            IDatabase database,
            ISystemClock systemClock,
            ILogger logger)
        {
            _provider = provider;
            _random = new();
            ProducerWrapper = producerWrapper;
            OptionsMonitor = optionsMonitor;
            Database = database;
            OptionsMonitor.OnChange((_, __) => ComputeNextExecution().ConfigureAwait(false).GetAwaiter().GetResult());
            SystemClock = systemClock;
            Logger = logger;
        }

        /// <inheritdoc/>
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Updating the scheduler hash with {0}.", Environment.MachineName);
            await Database.HashSetAsync(Options.SchedulersHashKey, Environment.MachineName, JsonSerializer.Serialize(SystemClock.UtcNow).Trim('"'));
            Logger.LogInformation("Scheduler list updated.");
            await ComputeNextExecution();
            await base.StartAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Updating the scheduler hash to remove {0}.", Environment.MachineName);
            await Database.HashDeleteAsync(Options.SchedulersHashKey, Environment.MachineName);
            Logger.LogInformation("Scheduler list updated.");
            await base.StopAsync(cancellationToken);
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var (scheduledFor, scheduledAt) = await GetScheduledFor();
                if (scheduledFor != Environment.MachineName || scheduledAt > SystemClock.UtcNow)
                {
                    await Task.Delay(Options.CronCheckIntervalMs, stoppingToken);
                    continue;
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
                                Logger.LogError(
                                    "Failed to deliver the data request for {0}: {1}",
                                    typeof(TRequest).FullName,
                                    JsonSerializer.Serialize(pr.Value)
                                );
                            }
                        }
                    });
                }
                Producer.Flush(stoppingToken);

                await ComputeNextExecution();
            }
        }

        /// <summary>
        /// Prepares the messages to be sent.
        /// </summary>
        /// <param name="context">The Db context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{IEnumerable{Message{string, TRequest}}}"/> that represents the asynchronous operation.</returns>
        protected abstract Task<IEnumerable<Message<string, TRequest>>> PrepareMessages(CryptofolioContext context, CancellationToken cancellationToken);

        private async Task ComputeNextExecution()
        {
            try
            {
                while (!await Database.LockTakeAsync(Options.SchedulersLockKey, Environment.MachineName, Options.SchedulersLockExpiration))
                {
                    await Task.Delay(Options.SchedulersLockCheckInterval);
                }

                Logger.LogInformation("Computing the next execution of the schedule.");

                var (_, scheduledAt) = await GetScheduledFor();
                if (scheduledAt < SystemClock.UtcNow)
                {
                    var cron = Options.Schedules[typeof(TRequest).FullName];
                    var nextScheduler = await GetNextScheduler();
                    var nextExecution = CronExpression.Parse(cron).GetNextOccurrence(SystemClock.UtcNow.UtcDateTime);
                    Logger.LogInformation("Next execution scheduled at {0} with cron: {1}", nextExecution, cron);
                    await Database.HashSetAsync(
                        string.Format(CultureInfo.InvariantCulture, Options.SchedulesHashKeyFormat, GetType().FullName),
                        new HashEntry[]
                        {
                            new(SchedulesHashScheduledForField, nextScheduler),
                            new(SchedulesHashScheduledAtField, JsonSerializer.Serialize(new DateTimeOffset(nextExecution.Value)).Trim('"'))
                        }
                    );
                }
                else
                {
                    Logger.LogInformation("Schedule allready computed.");
                }
            }
            finally
            {
                await Database.LockReleaseAsync(Options.SchedulersLockKey, Environment.MachineName);
            }
        }

        private async Task<(string, DateTimeOffset)> GetScheduledFor()
        {
            var hash = await Database.HashGetAllAsync(string.Format(CultureInfo.InvariantCulture, Options.SchedulesHashKeyFormat, GetType().FullName));
            var f1 = hash.SingleOrDefault(h => h.Name == SchedulesHashScheduledForField);
            var f2 = hash.SingleOrDefault(h => h.Name == SchedulesHashScheduledAtField);
            if (f1.Value.HasValue)
            {
                return (f1.Value.ToString(), JsonSerializer.Deserialize<DateTimeOffset>(f2.Value.ToString()));
            }
            return (null, default);
        }

        private async Task<string> GetNextScheduler()
        {
            var values = await Database.HashGetAllAsync(Options.SchedulersHashKey);
            if (values.All(v => v.Name == Environment.MachineName))
            {
                return Environment.MachineName;
            }
            var schedulers = values.Where(v => v.Name != Environment.MachineName).ToArray();
            return schedulers[_random.Next(schedulers.Length)].Name.ToString();
        }
    }
}
