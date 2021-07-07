using Confluent.Kafka;
using Cryptofolio.Collector.Job.Data;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Data;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Collector.Job.IntegrationTests.Data
{
    public class DataRequestSchedulerBaseTests : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory _factory;
        private readonly TestDataRequestScheduler _scheduler;
        private readonly Mock<ISystemClock> _systemClockMock;

        [Fact]
        public async Task ExecuteAsync_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            _scheduler.Messages = new[]
            {
                new Func<Message<string, TestDataRequest>>(() =>
                {
                    var guid = Guid.NewGuid().ToString();
                    return new Message<string, TestDataRequest>
                    {
                        Key = guid,
                        Value = new()
                        {
                            TraceIdentifier = guid,
                            Date = utcNow,
                            Id = Guid.NewGuid().ToString()
                        }
                    };
                })(),
                new Func<Message<string, TestDataRequest>>(() =>
                {
                    var guid = Guid.NewGuid().ToString();
                    return new Message<string, TestDataRequest>
                    {
                        Key = guid,
                        Value = new()
                        {
                            TraceIdentifier = guid,
                            Date = utcNow,
                            Id = Guid.NewGuid().ToString()
                        }
                    };
                })()
            };
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
            ISystemClock systemClock,
            ILogger logger
        ) : base(provider, producerWrapper, optionsMonitor, systemClock, logger)
        {
        }

        protected override Task<IEnumerable<Message<string, TestDataRequest>>> PrepareMessages(CryptofolioContext context, CancellationToken cancellationToken) =>
            Task.FromResult(Messages);
    }
}
