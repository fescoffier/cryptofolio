namespace Cryptofolio.Api.Commands
{
    public interface ITransactionCommand
    {
        public string Type { get; set; }

        public string ExchangeId { get; set; }

        public string CurrencyId { get; set; }

        public decimal Price { get; set; }

        public decimal Qty { get; set; }

        public string Source { get; set; }

        public string Destination { get; set; }
    }
}
