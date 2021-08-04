using Cryptofolio.Infrastructure.Entities;
using System;

namespace Cryptofolio.Api.IntegrationTests
{
    public class TestData
    {
        private string _backupUserId;
        private string _userId;
        public string UserId { get => _userId; }

        public Wallet Wallet1 { get; }

        public Wallet Wallet2 { get; }

        public Wallet Wallet3 { get; }

        public TestData()
        {
            _backupUserId = _userId = Guid.NewGuid().ToString();
            Wallet1 = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Wallet 1",
                Description = "Lorem ipsum dolor sit amet",
                UserId = UserId
            };
            Wallet2 = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Wallet 2",
                Description = "Lorem ipsum dolor sit amet",
                UserId = UserId
            };
            Wallet3 = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Wallet 3",
                Description = "Lorem ipsum dolor sit amet",
                UserId = UserId
            };
        }

        public void ChangUserId(string userId) => _userId = userId;

        public void RestoreUserId() => _userId = _backupUserId;
    }
}
