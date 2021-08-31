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
    /// Provides an implementation of <see cref="DataRequestSchedulerBase{TRequest}"/> to schedule <see cref="CurrencyTickerDataRequest"/>.
    /// </summary>
    public class CurrencyTickerDataRequestScheduler : DataRequestSchedulerBase<CurrencyTickerDataRequest>
    {
        /// <summary>
        /// Creates a new instance of <see cref="CurrencyTickerDataRequestScheduler"/>.
        /// </summary>
        /// <param name="provider">The service provider.</param>
        /// <param name="producerWrapper">The producer wrapper.</param>
        /// <param name="optionsMonitor">The options monitor.</param>
        /// <param name="database">The Redis database.</param>
        /// <param name="systemClock">The system clock.</param>
        /// <param name="logger">The logger.</param>
        public CurrencyTickerDataRequestScheduler(
            IServiceProvider provider,
            KafkaProducerWrapper<string, CurrencyTickerDataRequest> producerWrapper,
            IOptionsMonitor<DataRequestSchedulerOptions> optionsMonitor,
            IDatabase database,
            ISystemClock systemClock,
            ILogger logger
        ) : base(provider, producerWrapper, optionsMonitor, database, systemClock, logger)
        {
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<Message<string, CurrencyTickerDataRequest>>> PrepareMessages(CryptofolioContext context, CancellationToken cancellationToken)
        {
            var currencyKey = $"{typeof(CurrencyTickerDataRequestScheduler).FullName}:Currency";
            var vsCurrenciesKey = $"{typeof(CurrencyTickerDataRequestScheduler).FullName}:VsCurrencies";
            var settingsGroup = new[] { currencyKey, vsCurrenciesKey };
            var settings = await context.Settings
                .Where(s => settingsGroup.Contains(s.Group))
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            var guid = Guid.NewGuid().ToString();
            return settings
                .Where(s => s.Group == currencyKey)
                .Select(setting => new Message<string, CurrencyTickerDataRequest>
                {
                    Key = guid,
                    Value = new()
                    {
                        TraceIdentifier = guid,
                        Date = SystemClock.UtcNow,
                        Currency = setting.Value,
                        VsCurrencies = settings.Where(s => s.Key.StartsWith(setting.Key)).Select(s => s.Value).ToList()
                    }
                })
                .ToList();
        }
    }
}
