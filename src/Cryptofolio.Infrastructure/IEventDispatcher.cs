using System.Threading.Tasks;

namespace Cryptofolio.Infrastructure
{
    /// <summary>
    /// Provides an abstraction to dispatch event occurence.
    /// </summary>
    public interface IEventDispatcher
    {
        /// <summary>
        /// Dispatch the provided event.
        /// </summary>
        /// <param name="event">The event.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous task.</returns>
        Task DispatchAsync(IEvent @event);
    }
}
