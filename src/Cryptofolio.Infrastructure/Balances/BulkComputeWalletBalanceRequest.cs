using MediatR;
using System.Collections.Generic;

namespace Cryptofolio.Infrastructure.Balances
{
    /// <summary>
    /// Models a request to compute wallets balance in bulk.
    /// </summary>
    public class BulkComputeWalletBalanceRequest : IRequest
    {
        /// <summary>
        /// The list of assets ids that got impacted.
        /// </summary>
        public IEnumerable<string> AssetIds { get; set; }

        /// <summary>
        /// The list of currencies ids that got impacted.
        /// </summary>
        public IEnumerable<string> CurrencyIds { get; set; }
    }
}
