using Confluent.Kafka;
using Cryptofolio.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Cryptofolio.Infrastructure
{
    /// <summary>
    /// Provides an implementation of <see cref="IEventDispatcher"/> that dispatch event occurrences through Kafka.
    /// </summary>
    public class KafkaEventDispatcher : IEventDispatcher
    {
        private readonly KafkaProducerWrapper<string, IEvent> _producerWrapper;
        private readonly ILogger<KafkaEventDispatcher> _logger;

        private IProducer<string, IEvent> Producer => _producerWrapper.Producer;

        private KafkaOptions<IEvent> ProducerOptions => _producerWrapper.Options;

        /// <inheritdoc/>
        public async Task DispatchAsync(IEvent @event)
        {
            _logger.LogInformation("");

            var message = new Message<string, IEvent>
            {
                Key = Guid.NewGuid().ToString(),
                Value = @event
            };
            var pr = await Producer.ProduceAsync(ProducerOptions.Topic, message);
            if (pr.Status != PersistenceStatus.Persisted)
            {
                _logger.LogDebug("Event occurence dispatched to topic {0} at partition {1} and offset {2}.",
                    pr.Topic,
                    pr.TopicPartitionOffset.Partition,
                    pr.TopicPartitionOffset.Offset);
            }
            else if (pr.Status != PersistenceStatus.PossiblyPersisted)
            {
                
            }
            else
            {

            }
        }
    }
}
