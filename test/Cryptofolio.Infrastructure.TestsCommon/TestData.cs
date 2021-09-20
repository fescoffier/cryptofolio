using Cryptofolio.Infrastructure.Entities;
using System;

namespace Cryptofolio.Infrastructure.TestsCommon
{
    public class TestData
    {
        private readonly string _backupUserId;
        private string _userId;
        public string UserId { get => _userId; }

        public Asset BTC { get; }

        public AssetTicker BTC_USD_Ticker { get; }

        public AssetTicker BTC_EUR_Ticker { get; }

        public Asset ETH { get; }

        public AssetTicker ETH_USD_Ticker { get; }

        public AssetTicker ETH_EUR_Ticker { get; }

        public Currency USD { get; }

        public CurrencyTicker USD_EUR_Ticker { get; }

        public Currency EUR { get; }

        public CurrencyTicker EUR_USD_Ticker { get; }

        public Exchange Exchange1 { get; }

        public Exchange Exchange2 { get; }

        public Wallet Wallet1 { get; }

        public Wallet Wallet2 { get; }

        public Wallet Wallet3 { get; }

        public BuyOrSellTransaction Transaction1 { get; }

        public BuyOrSellTransaction Transaction2 { get; }

        public BuyOrSellTransaction Transaction3 { get; }

        public TransferTransaction Transaction4 { get; }

        public Holding Holding1 { get; }

        public Holding Holding2 { get; }

        public Holding Holding3 { get; }

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
            USD_EUR_Ticker = new()
            {
                Currency = USD,
                VsCurrency = EUR,
                Timestamp = DateTimeOffset.UtcNow,
                Value = 0.8m
            };
            EUR_USD_Ticker = new()
            {
                Currency = EUR,
                VsCurrency = USD,
                Timestamp = DateTimeOffset.UtcNow,
                Value = 1.2m
            };
            BTC_USD_Ticker = new()
            {
                Asset = BTC,
                VsCurrency = USD,
                Timestamp = DateTimeOffset.UtcNow,
                Value = 100_000m
            };
            ETH_USD_Ticker = new()
            {
                Asset = ETH,
                VsCurrency = USD,
                Timestamp = DateTimeOffset.UtcNow,
                Value = 20_000m
            };
            BTC_EUR_Ticker = new()
            {
                Asset = BTC,
                VsCurrency = EUR,
                Timestamp = DateTimeOffset.UtcNow,
                Value = BTC_USD_Ticker.Value * USD_EUR_Ticker.Value
            };
            ETH_EUR_Ticker = new()
            {
                Asset = ETH,
                VsCurrency = EUR,
                Timestamp = DateTimeOffset.UtcNow,
                Value = ETH_USD_Ticker.Value * USD_EUR_Ticker.Value
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
                Currency = USD,
                Selected = true,
                UserId = UserId
            };
            Wallet2 = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Wallet 2",
                Description = "Lorem ipsum dolor sit amet",
                Currency = USD,
                UserId = UserId
            };
            Wallet3 = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Wallet 3",
                Description = "Lorem ipsum dolor sit amet",
                Currency = EUR,
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
                Price = BTC_USD_Ticker.Value,
                Qty = 10,
                InitialValue = 10 * BTC_USD_Ticker.Value,
                Note = "Lorem ipsum dolor sit amet"
            };
            Transaction2 = new BuyOrSellTransaction
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                Asset = BTC,
                Wallet = Wallet1,
                Exchange = Exchange1,
                Type = InfrastructureConstants.Transactions.Types.Sell,
                Currency = USD,
                Price = BTC_USD_Ticker.Value * 1.2759m,
                Qty = 2,
                InitialValue = 2 * BTC_USD_Ticker.Value * 1.2759m,
                Note = "Lorem ipsum dolor sit amet"
            };
            Transaction3 = new BuyOrSellTransaction
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                Asset = BTC,
                Wallet = Wallet2,
                Exchange = Exchange1,
                Type = InfrastructureConstants.Transactions.Types.Buy,
                Currency = EUR,
                Price = BTC_EUR_Ticker.Value,
                Qty = 100,
                InitialValue = 100 * BTC_EUR_Ticker.Value,
                Note = "Lorem ipsum dolor sit amet"
            };
            Transaction4 = new TransferTransaction
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                Asset = ETH,
                Wallet = Wallet3,
                Exchange = Exchange2,
                Source = InfrastructureConstants.Transactions.Sources.MyExchange,
                Destination = InfrastructureConstants.Transactions.Destinations.MyWallet,
                Qty = 50,
                InitialValue = 50 * ETH_EUR_Ticker.Value,
                Note = "Lorem ipsum dolor sit amet"
            };
            Holding1 = new()
            {
                Id = Guid.NewGuid().ToString(),
                Asset = BTC,
                Wallet = Wallet1,
                Qty = Transaction1.Qty - Transaction2.Qty,
                InitialValue = Transaction1.InitialValue - (Transaction1.InitialValue * Transaction2.Qty / Transaction1.Qty)
            };
            Holding2 = new()
            {
                Id = Guid.NewGuid().ToString(),
                Asset = BTC,
                Wallet = Wallet2,
                Qty = Transaction3.Qty,
                InitialValue = Transaction3.InitialValue * EUR_USD_Ticker.Value
            };
            Holding3 = new()
            {
                Id = Guid.NewGuid().ToString(),
                Asset = ETH,
                Wallet = Wallet3,
                Qty = Transaction4.Qty,
                InitialValue = Transaction4.InitialValue
            };
            Wallet1.InitialValue = Holding1.InitialValue;
            Wallet2.InitialValue = Holding2.InitialValue;
            Wallet3.InitialValue = Holding3.InitialValue;
        }

        public void ChangUserId(string userId) => _userId = userId;

        public void RestoreUserId() => _userId = _backupUserId;
    }
}
