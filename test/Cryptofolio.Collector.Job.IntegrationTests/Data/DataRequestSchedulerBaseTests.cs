using Confluent.Kafka;
using Cryptofolio.Collector.Job.Data;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Collector.Job.IntegrationTests.Data
{
    public class DataRequestSchedulerBaseTests : IClassFixture<WebApplicationFactory>
    {
        private readonly TestDataRequestScheduler _scheduler;
        private readonly KafkaConsumerWrapper<string, TestDataRequest> _consumerWrapper;

        public DataRequestSchedulerBaseTests(WebApplicationFactory factory)
        {
            _scheduler = factory.Services.GetRequiredService<TestDataRequestScheduler>();
            _consumerWrapper = factory.Services.GetRequiredService<KafkaConsumerWrapper<string, TestDataRequest>>();
        }

        [Fact]
        public async Task ExecuteAsync_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _scheduler.Messages = new[]
            {
                new Message<string, TestDataRequest>
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = new()
                    {
                        TraceIdentifier = Guid.NewGuid().ToString(),
                        Date = utcNow,
                        Id = Guid.NewGuid().ToString()
                    }
                },
                new Message<string, TestDataRequest>
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = new()
                    {
                        TraceIdentifier = Guid.NewGuid().ToString(),
                        Date = utcNow,
                        Id = Guid.NewGuid().ToString()
                    }
                }
            };

            // Act
            await _scheduler.StartAsync(CancellationToken.None);
            await Task.Delay(TimeSpan.FromSeconds(65));
            await _scheduler.StopAsync(CancellationToken.None);

            // Assert
            var messages = new List<TestDataRequest>();
            _consumerWrapper.Consumer.Subscribe(_consumerWrapper.Options.Topic);
            var cr = _consumerWrapper.Consumer.Consume();
            while (!cr.IsPartitionEOF)
            {
                messages.Add(cr.Message.Value);
                cr = _consumerWrapper.Consumer.Consume();
            }
            _consumerWrapper.Consumer.Unsubscribe();
            messages.Should().BeEquivalentTo(_scheduler.Messages.Select(m => m.Value).ToList());
        }
    }

    public class TestDataRequest : DataRequest
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }

    public class TestDataRequestScheduler : DataRequestSchedulerBase<TestDataRequest>
    {
        public IEnumerable<Message<string, TestDataRequest>> Messages { get; set; }

        public TestDataRequestScheduler(
            IServiceProvider provider,
            KafkaProducerWrapper<string, TestDataRequest> producerWrapper,
            IOptionsMonitor<DataRequestSchedulerOptions> optionsMonitor,
            IDatabase database,
            ISystemClock systemClock,
            ILogger<TestDataRequestScheduler> logger
        ) : base(provider, producerWrapper, optionsMonitor, database, systemClock, logger)
        {
        }

        protected override Task<IEnumerable<Message<string, TestDataRequest>>> PrepareMessages(CryptofolioContext context, CancellationToken cancellationToken) =>
            Task.FromResult(Messages);
    }
}
