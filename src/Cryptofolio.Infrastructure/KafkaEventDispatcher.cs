using Confluent.Kafka;
using Microsoft.Extensions.Logging;
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

        /// <summary>
        /// Creates a new instance of <see cref="KafkaEventDispatcher"/>.
        /// </summary>
        /// <param name="producerWrapper">The producer wrapper.</param>
        /// <param name="logger">The logger.</param>
        public KafkaEventDispatcher(KafkaProducerWrapper<string, IEvent> producerWrapper, ILogger<KafkaEventDispatcher> logger)
        {
            _producerWrapper = producerWrapper;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task DispatchAsync(IEvent @event)
        {
            _logger.LogInformation("Dispatching an event occurence to Kafka.");
            _logger.LogTraceObject("Event", @event);

            var message = new Message<string, IEvent>
            {
                Key = @event.Id,
                Value = @event
            };
            var pr = await Producer.ProduceAsync(ProducerOptions.Topic, message);
            if (pr.Status == PersistenceStatus.Persisted)
            {
                _logger.LogDebug("Event occurence dispatched to topic {0} at partition {1} and offset {2}.",
                    pr.Topic,
                    pr.TopicPartitionOffset.Partition,
                    pr.TopicPartitionOffset.Offset);
            }
            else if (pr.Status == PersistenceStatus.PossiblyPersisted)
            {
                _logger.LogWarning("Event occurence dispatched to topic {0} at partition {1} and offset {2}, but no acknowledgement was received by the broker.",
                    pr.Topic,
                    pr.TopicPartitionOffset.Partition,
                    pr.TopicPartitionOffset.Offset);
            }
            else
            {
                _logger.LogError("Event occurence can't be dispatched to topic {0} at partition {1} and offset {2}.",
                    pr.Topic,
                    pr.TopicPartitionOffset.Partition,
                    pr.TopicPartitionOffset.Offset);
            }
        }
    }
}
