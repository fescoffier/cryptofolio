using System;

namespace Cryptofolio.Api.Commands
{
    public interface ITransactionCommand
    {
        public string Type { get; set; }

        public DateTimeOffset Date { get; set; }

        public string WalletId { get; set; }

        public string AssetId { get; set; }

        public string ExchangeId { get; set; }

        public string Currency { get; set; }

        public decimal Price { get; set; }

        public decimal Qty { get; set; }

        public string Source { get; set; }

        public string Destination { get; set; }

        public string Note { get; set; }
    }
}
