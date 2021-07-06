using Confluent.Kafka;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Collector.Job.Data
{
    public abstract class DataRequestSchedulerBase<TRequest> : BackgroundService where TRequest : DataRequest
    {
        private readonly IServiceProvider _provider;

        protected KafkaProducerWrapper<string, TRequest> ProducerWrapper { get; }

        protected IProducer<string, TRequest> Producer => ProducerWrapper.Producer;

        protected KafkaOptions<TRequest> ProducerOptions => ProducerWrapper.Options;

        protected TimeSpan Interval { get; }

        protected ILogger Logger { get; }

        protected DataRequestSchedulerBase(
            IServiceProvider provider,
            KafkaProducerWrapper<string, TRequest> producerWrapper,
            TimeSpan interval,
            ILogger logger
        )
        {
            _provider = provider;
            ProducerWrapper = producerWrapper;
            Interval = interval;
            Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
        }

        protected abstract IEnumerable<Message<string, TRequest>> PrepareMessages(CryptofolioContext context, CancellationToken cancellationToken);
    }
}
