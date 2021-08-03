using Cryptofolio.Infrastructure.Entities;
using System;

namespace Cryptofolio.Api.IntegrationTests
{
    public class TestData
    {
        public string UserId1 { get; } = Guid.NewGuid().ToString();

        public string UserId2 { get; } = Guid.NewGuid().ToString();

        public Wallet Wallet1 { get; }

        public Wallet Wallet2 { get; }

        public TestData()
        {
            Wallet1 = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Wallet 1",
                Description = "Lorem ipsum dolor sit amet",
                UserId = UserId1
            };
            Wallet2 = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Wallet 2",
                Description = "Lorem ipsum dolor sit amet",
                UserId = UserId2
            };
        }
    }
}
