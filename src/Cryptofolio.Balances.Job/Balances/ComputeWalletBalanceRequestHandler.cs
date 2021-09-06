using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Balances;
using Cryptofolio.Infrastructure.Caching;
using Cryptofolio.Infrastructure.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Balances.Job.Balances
{
    /// <summary>
    /// Provides an implementation of <see cref="IRequestHandler{ComputeWalletBalanceRequest}"/>.
    /// It computes the value of a wallet, its holdings and transactions.
    /// </summary>
    public class ComputeWalletBalanceRequestHandler : IRequestHandler<ComputeWalletBalanceRequest>
    {
        private readonly CryptofolioContext _context;
        private readonly AssetTickerCache _tickerCache;
        private readonly ILogger<ComputeWalletBalanceRequestHandler> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="ComputeWalletBalanceRequestHandler"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="tickerCache">The ticker cache.</param>
        /// <param name="logger">The logger.</param>
        public ComputeWalletBalanceRequestHandler(
            CryptofolioContext context,
            AssetTickerCache tickerCache,
            ILogger<ComputeWalletBalanceRequestHandler> logger)
        {
            _context = context;
            _tickerCache = tickerCache;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(ComputeWalletBalanceRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Computing the balance of the {0} wallet.", request.WalletId);

            var wallet = await _context.Wallets
                .Include(w => w.Currency)
                .Include(w => w.Holdings)
                .SingleOrDefaultAsync(w => w.Id == request.WalletId, cancellationToken);
            if (wallet != null)
            {
                var tickers = new Dictionary<TickerPair, decimal>();
                var transactions = await _context.Transactions
                    .Include(t => t.Asset)
                    .Include(nameof(BuyOrSellTransaction.Currency))
                    .Where(t => t.Wallet.Id == request.WalletId)
                    .ToListAsync(cancellationToken);

                _logger.LogDebug("Computing transactions value.");
                _logger.LogDebug("There is {0} transactions in the {1} wallet.", transactions.Count, request.WalletId);

                foreach (var transaction in transactions)
                {
                    if (transaction is BuyOrSellTransaction bst)
                    {
                        var ticker = await GetAssetTicker(new(transaction.Asset.Symbol, bst.Currency.Code));
                        bst.CurrentValue = bst.Qty * ticker;
                        if (bst.InitialValue > 0)
                        {
                            bst.Change = (bst.CurrentValue - bst.InitialValue) / bst.InitialValue * 100;
                        }
                    }
                    else if (transaction is TransferTransaction tft)
                    {
                        var ticker = await GetAssetTicker(new(transaction.Asset.Symbol, wallet.Currency.Code));
                        tft.CurrentValue = tft.Qty * ticker;
                        if (tft.InitialValue > 0)
                        {
                            tft.Change = (tft.CurrentValue - tft.InitialValue) / tft.InitialValue * 100;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Unsupported transaction type: {0}", transaction.GetType().AssemblyQualifiedName);
                    }
                }

                _logger.LogDebug("Computing holdings value.");

                foreach (var holding in wallet.Holdings)
                {
                    var ticker = await GetAssetTicker(new(holding.Asset.Symbol, wallet.Currency.Code));
                    holding.CurrentValue = holding.Qty * ticker;
                    if (holding.InitialValue > 0)
                    {
                        holding.Change = (holding.CurrentValue - holding.InitialValue) / holding.InitialValue * 100;
                    }
                }

                _logger.LogDebug("Computing wallet balance.");

                wallet.CurrentValue = wallet.Holdings.Sum(h => h.CurrentValue);
                if (wallet.InitialValue > 0)
                {
                    wallet.Change = (wallet.CurrentValue - wallet.InitialValue) / wallet.InitialValue * 100;
                }

                async Task<decimal> GetAssetTicker(TickerPair pair)
                {
                    if (!tickers.TryGetValue(pair, out var ticker))
                    {
                        var t = await _tickerCache.GetTickersAsync(pair);
                        if (t.Any())
                        {
                            ticker = t.First().Value;
                            tickers.Add(pair, ticker);
                        }
                    }
                    return ticker;
                }
            }
            else
            {
                _logger.LogWarning("The wallet {0} does not exist.");
            }

            _logger.LogInformation("Compute balance request for the {0} wallet handled.", request.WalletId);

            return Unit.Value;
        }
    }
}
