using Confluent.Kafka;
using System.Text.Json;

namespace Cryptofolio.Infrastructure
{
    /// <summary>
    /// Configuration options for Kafka related services.
    /// </summary>
    public class KafkaOptions<TMessage>
    {
        /// <summary>
        /// The topic associated to the <see cref="TMessage"/>.
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// The <see cref="JsonSerializerOptions"/> used to serialize/deserialize values in the topic.
        /// </summary>
        public JsonSerializerOptions ValueSerilializerOptions { get; set; }
    }

    /// <summary>
    /// Configuration options for Kafka producers.
    /// </summary>
    public class KafkaProducerOptions<TMessage> : KafkaOptions<TMessage>
    {
        /// <summary>
        /// The producer config.
        /// </summary>
        public ProducerConfig Config { get; set; }
    }

    /// <summary>
    /// Configuration options for Kafka consumers.
    /// </summary>
    public class KafkaConsumerOptions<TMessage> : KafkaOptions<TMessage>
    {
        /// <summary>
        /// The consumer config.
        /// </summary>
        public ConsumerConfig Config { get; set; }
    }
}
