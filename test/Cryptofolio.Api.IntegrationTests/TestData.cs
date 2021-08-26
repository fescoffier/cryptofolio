using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Entities;
using System;

namespace Cryptofolio.Api.IntegrationTests
{
    public class TestData
    {
        private string _backupUserId;
        private string _userId;
        public string UserId { get => _userId; }

        public Asset BTC { get; }

        public Asset ETH { get; }

        public Currency USD { get; }

        public Currency EUR { get; }

        public Exchange Exchange1 { get; }

        public Exchange Exchange2 { get; }

        public Wallet Wallet1 { get; }

        public Wallet Wallet2 { get; }

        public Wallet Wallet3 { get; }

        public BuyOrSellTransaction Transaction1 { get; }

        public BuyOrSellTransaction Transaction2 { get; }

        public BuyOrSellTransaction Transaction3 { get; }

        public TransferTransaction Transaction4 { get; }

        public TestData()
        {
            _backupUserId = _userId = Guid.NewGuid().ToString();
            BTC = new()
            {
                Id = "bitcoin",
                Name = "Bitcoin",
                Symbol = "btc",
                Description = "Lorem ipsum dolor sit amet"
            };
            ETH = new()
            {
                Id = "ethereum",
                Name = "Ethereum",
                Symbol = "eth",
                Description = "Lorem ipsum dolor sit amet"
            };
            USD = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "United States Dolar",
                Code = "usd",
                Symbol = "$"
            };
            EUR = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Euro Members Countries",
                Code = "eur",
                Symbol = "€"
            };
            Exchange1 = new()
            {
                Id = "exchange1",
                Name = "Exchange 1",
                Image = "https://picsum.photos/200/300",
                Url = "https://exchange1.com",
                Description = "Lorem ipsum dolor sit amet",
                YearEstablished = DateTime.Today.Year,
            };
            Exchange2 = new()
            {
                Id = "exchange2",
                Name = "Exchange 2",
                Image = "https://picsum.photos/200/300",
                Url = "https://exchange1.com",
                Description = "Lorem ipsum dolor sit amet",
                YearEstablished = DateTime.Today.Year,
            };
            Wallet1 = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Wallet 1",
                Description = "Lorem ipsum dolor sit amet",
                Selected = true,
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
            Transaction1 = new BuyOrSellTransaction
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                Asset = BTC,
                Wallet = Wallet1,
                Exchange = Exchange1,
                Type = InfrastructureConstants.Transactions.Types.Buy,
                Currency = USD,
                Price = 1000,
                Qty = 10,
                Note = "Lorem ipsum dolor sit amet"
            };
            Transaction2 = new BuyOrSellTransaction
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow.AddMinutes(-2),
                Asset = BTC,
                Wallet = Wallet1,
                Exchange = Exchange1,
                Type = InfrastructureConstants.Transactions.Types.Sell,
                Currency = USD,
                Price = 1500,
                Qty = 10,
                Note = "Lorem ipsum dolor sit amet"
            };
            Transaction3 = new BuyOrSellTransaction
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow.AddMinutes(-4),
                Asset = BTC,
                Wallet = Wallet2,
                Exchange = Exchange1,
                Type = InfrastructureConstants.Transactions.Types.Buy,
                Currency = EUR,
                Price = 150,
                Qty = 100,
                Note = "Lorem ipsum dolor sit amet"
            };
            Transaction4 = new TransferTransaction
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow.AddMinutes(-5),
                Asset = BTC,
                Wallet = Wallet3,
                Exchange = Exchange2,
                Source = InfrastructureConstants.Transactions.Sources.MyExchange,
                Destination = InfrastructureConstants.Transactions.Destinations.MyWallet,
                Qty = 50,
                Note = "Lorem ipsum dolor sit amet"
            };
        }

        public void ChangUserId(string userId) => _userId = userId;

        public void RestoreUserId() => _userId = _backupUserId;
    }
}
