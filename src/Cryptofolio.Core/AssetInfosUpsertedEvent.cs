using System;

namespace Cryptofolio.Core
{
    public class AssetInfosUpsertedEvent : IEvent
    {
        public DateTimeOffset Date { get; init; }

        public string UserId { get; init; }

        public string Username { get; init; }

        public string Category => EventConstants.Categories.Asset;

        public Asset Asset { get; init; }
    }
}
