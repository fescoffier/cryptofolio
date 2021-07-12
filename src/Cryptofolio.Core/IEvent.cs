using System;

namespace Cryptofolio.Core
{
    /// <summary>
    /// Provides an abstraction to model an event.
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// The date at which the event occured.
        /// </summary>
        public DateTimeOffset Date { get; }

        /// <summary>
        /// The user's id that triggered the event.
        /// </summary>
        public string UserId { get; }

        /// <summary>
        /// The user's username that triggered the event.
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// The event category.
        /// </summary>
        public string Category { get; }
    }
}
