using Confluent.Kafka;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Collector.Job.Data
{
    /// <summary>
    /// Provides an implementation of <see cref="DataRequestSchedulerBase{AssetDataRequest}"/> to schedule <see cref="AssetDataRequest"/>.
    /// </summary>
    public class AssetDataRequestScheduler : DataRequestSchedulerBase<AssetDataRequest>
    {
        /// <summary>
        /// Creates a new instance of <see cref="AssetDataRequestScheduler"/>.
        /// </summary>
        /// <param name="provider">The service provider.</param>
        /// <param name="producerWrapper">The producer wrapper.</param>
        /// <param name="optionsMonitor">The options monitor.</param>
        /// <param name="database">The Redis database.</param>
        /// <param name="systemClock">The system clock.</param>
        /// <param name="logger">The logger.</param>
        public AssetDataRequestScheduler(
            IServiceProvider provider,
            KafkaProducerWrapper<string, AssetDataRequest> producerWrapper,
            IOptionsMonitor<DataRequestSchedulerOptions> optionsMonitor,
            IDatabase database,
            ISystemClock systemClock,
            ILogger<AssetDataRequestScheduler> logger
        ) : base(provider, producerWrapper, optionsMonitor, database, systemClock, logger)
        {
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<Message<string, AssetDataRequest>>> PrepareMessages(CryptofolioContext context, CancellationToken cancellationToken)
        {
            var settings = await context.Settings
                .Where(s => s.Group == typeof(AssetDataRequestScheduler).FullName)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            return settings
                .Select(setting =>
                {
                    var guid = Guid.NewGuid().ToString();
                    return new Message<string, AssetDataRequest>
                    {
                        Key = guid,
                        Value = new()
                        {
                            TraceIdentifier = guid,
                            Date = SystemClock.UtcNow,
                            Id = setting.Value
                        }
                    };
                })
                .ToList();
        }
    }
}
