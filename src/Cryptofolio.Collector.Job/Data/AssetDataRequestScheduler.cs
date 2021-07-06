using Confluent.Kafka;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Collector.Job.Data
{
    public class AssetDataRequestScheduler : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly KafkaProducerWrapper<string, AssetDataRequest> _producerWrapper;
        private readonly ISystemClock _systemClock;
        private readonly IOptionsMonitor<DataRequestOptions> _optionsMonitor;
        private readonly ILogger<AssetDataRequestScheduler> _logger;

        private IProducer<string, AssetDataRequest> Producer => _producerWrapper.Producer;

        private KafkaOptions<AssetDataRequest> ProducerOptions => _producerWrapper.Options;

        private DataRequestOptions Options => _optionsMonitor.CurrentValue;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = TimeSpan.FromMinutes(Options.AssetScheduleIntervalMinutes);
            do
            {
                using var scope = _provider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
                var settings = await context.Settings
                    .Where(s => s.Group == "AssetDataRequest")
                    .AsNoTracking()
                    .ToListAsync(stoppingToken);
                foreach (var setting in settings)
                {
                    var guid = Guid.NewGuid().ToString();
                    var message = new Message<string, AssetDataRequest>
                    {
                        Key = guid,
                        Value = new AssetDataRequest
                        {
                            TraceIdentifier = guid,
                            Date = _systemClock.UtcNow,
                            Id = setting.Value
                        }
                    };
                    Producer.Produce(
                        ProducerOptions.Topic,
                        message,
                        dr =>
                        {
                            
                        }
                    );
                }
                Producer.Flush();
                await Task.Delay(interval);
            } while (!stoppingToken.IsCancellationRequested);
        }
    }
}
