using Cryptofolio.Infrastructure;
using System;

namespace Cryptofolio.Handlers.Job.IntegrationTests
{
    public class FakeEvent : IEvent
    {
        public string Id { get; init; }

        public DateTimeOffset Date { get; init; }

        public string UserId { get; init; }

        public string Category => "Test";

        public string Property1 { get; init; }

        public string Property2 { get; init; }
    }
}
