using MediatR;

namespace Cryptofolio.Infrastructure.Balances
{
    /// <summary>
    /// Models a a request to compute the balance of a wallet.
    /// </summary>
    public class ComputeWalletBalanceRequest : IRequest
    {
        /// <summary>
        /// The wallet id.
        /// </summary>
        public string WalletId { get; set; }
    }
}
