using MediatR;

namespace Cryptofolio.Infrastructure.Balances
{
    /// <summary>
    /// Models a response after the computation of a wallet balance.
    /// </summary>
    public class ComputeWalletBalanceResponse : IRequest
    {
        /// <summary>
        /// The wallet id.
        /// </summary>
        public string WalletId { get; set; }
    }
}
