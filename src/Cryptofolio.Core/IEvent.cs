using System;

namespace Cryptofolio.Core
{
    public interface IEvent
    {
        public DateTimeOffset Date { get; }

        public string UserId { get; }

        public string Username { get; }

        public string Category { get; }
    }
}
