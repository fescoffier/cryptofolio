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
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Collector.Job.IntegrationTests.Data
{
    public class DataRequestSchedulerBaseTests : SchedulerTestBase, IClassFixture<WebApplicationFactory>
    {
        private readonly TestDataRequestScheduler _scheduler;
        private readonly KafkaProducerWrapper<string, TestDataRequest> _producerWrapper;
        private readonly KafkaConsumerWrapper<string, TestDataRequest> _consumerWrapper;
        private readonly IDatabase _database;
        private readonly DataRequestSchedulerOptions _options;

        public DataRequestSchedulerBaseTests(WebApplicationFactory factory)
        {
            _scheduler = factory.Services.GetRequiredService<TestDataRequestScheduler>();
            _producerWrapper = factory.Services.GetRequiredService<KafkaProducerWrapper<string, TestDataRequest>>();
            _consumerWrapper = factory.Services.GetRequiredService<KafkaConsumerWrapper<string, TestDataRequest>>();
            _database = factory.Services.GetRequiredService<IDatabase>();
            _options = factory.Services.GetRequiredService<IOptions<DataRequestSchedulerOptions>>().Value;
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
            _producerWrapper.Options.Topic = _consumerWrapper.Options.Topic = Guid.NewGuid().ToString();

            // Act
            await _scheduler.StartAsync(CancellationToken.None);
            await Task.Delay(SchedulerDelay);
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

        [Fact]
        public async Task ExecuteAsync_WithStaleScheduler_Test()
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
            _producerWrapper.Options.Topic = _consumerWrapper.Options.Topic = Guid.NewGuid().ToString();

            // Act
            await _scheduler.StartAsync(CancellationToken.None);
            _database.HashSet(
                string.Format(CultureInfo.InvariantCulture, _options.SchedulesHashKeyFormat, _scheduler.GetType().FullName),
                new HashEntry[]
                {
                    new(DataRequestSchedulerBase<TestDataRequest>.SchedulesHashScheduledForField, Guid.NewGuid().ToString()),
                    new(DataRequestSchedulerBase<TestDataRequest>.SchedulesHashScheduledAtField, JsonSerializer.Serialize(DateTimeOffset.UtcNow.AddMinutes(10)))
                }
            );
            await Task.Delay(SchedulerDelay);
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
