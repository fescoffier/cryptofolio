using Cryptofolio.Collector.Job.Data;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Data;
using Cryptofolio.Infrastructure.Entities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Collector.Job.IntegrationTests.Data
{
    public class ExchangeDataRequestSchedulerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory _factory;
        private readonly ExchangeDataRequestScheduler _scheduler;
        private readonly KafkaConsumerWrapper<string, ExchangeDataRequest> _consumerWrapper;

        public ExchangeDataRequestSchedulerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scheduler = factory.Services.GetRequiredService<ExchangeDataRequestScheduler>();
            _consumerWrapper = factory.Services.GetRequiredService<KafkaConsumerWrapper<string, ExchangeDataRequest>>();
        }

        [Fact]
        public async Task PrepareMessages_Test()
        {
            // Setup
            var settings = new[]
            {
                new Setting
                {
                    Key = Guid.NewGuid().ToString(),
                    Group = typeof(ExchangeDataRequestScheduler).FullName,
                    Value = "gdax"
                },
                new Setting
                {
                    Key = Guid.NewGuid().ToString(),
                    Group = typeof(ExchangeDataRequestScheduler).FullName,
                    Value = "binance"
                }
            };
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            context.Settings.AddRange(settings);
            context.SaveChanges();

            // Act
            await _scheduler.StartAsync(CancellationToken.None);
            await Task.Delay(TimeSpan.FromMinutes(1.5));
            await _scheduler.StopAsync(CancellationToken.None);

            // Asset
            var messages = new List<ExchangeDataRequest>();
            _consumerWrapper.Consumer.Subscribe(_consumerWrapper.Options.Topic);
            var cr = _consumerWrapper.Consumer.Consume();
            while (!cr.IsPartitionEOF)
            {
                messages.Add(cr.Message.Value);
                cr = _consumerWrapper.Consumer.Consume();
            }
            _consumerWrapper.Consumer.Unsubscribe();
            messages
                .Should()
                .BeEquivalentTo(new[]
                {
                    new AssetDataRequest
                    {
                        Id = "gdax"
                    },
                    new AssetDataRequest
                    {
                        Id = "binance"
                    }
                },
                options => options.Excluding(p => p.TraceIdentifier).Excluding(p => p.Date));
        }
    }
}
