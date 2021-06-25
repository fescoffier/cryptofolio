using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System;

namespace Cryptofolio.Infrastructure
{
    /// <summary>
    /// Provides a wrapper for <see cref="IConsumer{TKey, TValue}"/> and its <see cref="KafkaConsumerOptions"/>.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    public class KafkaConsumerWrapper<TKey, TValue>
    {
        /// <summary>
        /// The consumer instance.
        /// </summary>
        public IConsumer<TKey, TValue> Consumer { get; }

        /// <summary>
        /// The consumer's options monitor.
        /// </summary>
        public IOptionsMonitor<KafkaConsumerOptions<TValue>> OptionsMonitor { get; }

        /// <summary>
        /// The consumer's options.
        /// </summary>
        public KafkaConsumerOptions<TValue> Options => OptionsMonitor.CurrentValue;

        /// <summary>
        /// Creates a new instance of <see cref="KafkaConsumerWrapper{TKey, TValue}"/>.
        /// </summary>
        /// <param name="consumer">The wrapped consumer.</param>
        /// <param name="optionsMonitor">The options monitor.</param>
        public KafkaConsumerWrapper(IConsumer<TKey, TValue> consumer, IOptionsMonitor<KafkaConsumerOptions<TValue>> optionsMonitor)
        {
            Consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            OptionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        }
    }
}
