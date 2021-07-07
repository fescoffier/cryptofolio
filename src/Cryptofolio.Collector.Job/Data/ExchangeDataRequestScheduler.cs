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
    /// Provides an implementation of <see cref="DataRequestSchedulerBase{ExchangeDataRequest}"/> to schedule <see cref="ExchangeDataRequest"/>.
    /// </summary>
    public class ExchangeDataRequestScheduler : DataRequestSchedulerBase<ExchangeDataRequest>
    {
        /// <summary>
        /// Creates a new instance of <see cref="ExchangeDataRequestScheduler"/>.
        /// </summary>
        /// <param name="provider">The service provider.</param>
        /// <param name="producerWrapper">The producer wrapper.</param>
        /// <param name="optionsMonitor">The options monitor.</param>
        /// <param name="systemClock">The system clock.</param>
        /// <param name="logger">The logger.</param>
        public ExchangeDataRequestScheduler(
            IServiceProvider provider,
            KafkaProducerWrapper<string, ExchangeDataRequest> producerWrapper,
            IOptionsMonitor<DataRequestSchedulerOptions> optionsMonitor,
            ISystemClock systemClock,
            ILogger<ExchangeDataRequestScheduler> logger
        ) : base(provider, producerWrapper, optionsMonitor, systemClock, logger)
        {
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<Message<string, ExchangeDataRequest>>> PrepareMessages(CryptofolioContext context, CancellationToken cancellationToken)
        {
            var settings = await context.Settings
                .Where(s => s.Group == typeof(ExchangeDataRequestScheduler).FullName)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            return settings
                .Select(setting =>
                {
                    var guid = Guid.NewGuid().ToString();
                    return new Message<string, ExchangeDataRequest>
                    {
                        Key = guid,
                        Value = new ExchangeDataRequest
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
