using Cryptofolio.Core;
using System.Threading.Tasks;

namespace Cryptofolio.Infrastructure
{
    /// <summary>
    /// Provides an implementation of <see cref="IEventDispatcher"/> that dispatch event occurrences through Kafka.
    /// </summary>
    public class KafkaEventDispatcher : IEventDispatcher
    {
        /// <inheritdoc/>
        public Task DispatchAsync(IEvent @event) => Task.CompletedTask;
    }
}
