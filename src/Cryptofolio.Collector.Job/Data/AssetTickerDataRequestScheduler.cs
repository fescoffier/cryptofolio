using Confluent.Kafka;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Collector.Job.Data
{
    /// <summary>
    /// Provides an implementation of <see cref="DataRequestSchedulerBase{AssetTickerDataRequest}"/> to schedule <see cref="AssetTickerDataRequest"/>.
    /// </summary>
    public class AssetTickerDataRequestScheduler : DataRequestSchedulerBase<AssetTickerDataRequest>
    {
        /// <summary>
        /// Creates a new instance of <see cref="AssetTickerDataRequestScheduler"/>.
        /// </summary>
        /// <param name="provider">The service provider.</param>
        /// <param name="producerWrapper">The producer wrapper.</param>
        /// <param name="optionsMonitor">The options monitor.</param>
        /// <param name="systemClock">The system clock.</param>
        /// <param name="logger">The logger.</param>
        public AssetTickerDataRequestScheduler(
            IServiceProvider provider,
            KafkaProducerWrapper<string, AssetTickerDataRequest> producerWrapper,
            IOptionsMonitor<DataRequestSchedulerOptions> optionsMonitor,
            ISystemClock systemClock,
            ILogger<AssetTickerDataRequestScheduler> logger
        ) : base(provider, producerWrapper, optionsMonitor, systemClock, logger)
        {
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<Message<string, AssetTickerDataRequest>>> PrepareMessages(CryptofolioContext context, CancellationToken cancellationToken)
        {
            var idsKey = $"{typeof(AssetTickerDataRequestScheduler).FullName}:Ids";
            var vsCurrenciesKey = $"{typeof(AssetTickerDataRequestScheduler).FullName}:VsCurrencies";
            var settingsGroup = new[] { idsKey, vsCurrenciesKey };
            var settings = await context.Settings
                .Where(s => settingsGroup.Contains(s.Group))
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            var guid = Guid.NewGuid().ToString();
            return new[]
            {
                new Message<string, AssetTickerDataRequest>
                {
                    Key = guid,
                    Value = new AssetTickerDataRequest
                    {
                        TraceIdentifier = guid,
                        Date = SystemClock.UtcNow,
                        Ids = settings.Where(s => s.Group == idsKey).Select(s => s.Value).ToList(),
                        VsCurrencies = settings.Where(s => s.Group == vsCurrenciesKey).Select(s => s.Value).ToList()
                    }
                }
            };
        }
    }
}
