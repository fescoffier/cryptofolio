using Cryptofolio.App.Hubs;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Balances;
using Cryptofolio.Infrastructure.Caching;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.App.Balances
{
    /// <summary>
    /// Provides an implementation of <see cref="IRequestHandler{ComputeWalletBalanceResponse}"/>.
    /// It notifies the client that its wallet balance changed.
    /// </summary>
    public class ComputeWalletBalanceResponseHandler : IRequestHandler<ComputeWalletBalanceResponse>
    {
        private readonly CryptofolioContext _context;
        private readonly AssetTickerCache _tickerCache;
        private readonly IHubContext<DashboardHub> _hubContext;
        private readonly ILogger<ComputeWalletBalanceResponse> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="ComputeWalletBalanceResponseHandler"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="tickerCache">The ticker cache.</param>
        /// <param name="hubContext">The hub context.</param>
        /// <param name="logger">The logger.</param>
        public ComputeWalletBalanceResponseHandler(
            CryptofolioContext context,
            AssetTickerCache tickerCache,
            IHubContext<DashboardHub> hubContext,
            ILogger<ComputeWalletBalanceResponse> logger)
        {
            _context = context;
            _tickerCache = tickerCache;
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(ComputeWalletBalanceResponse response, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Notifying the wallet {0} owner that its balance changed.", response.WalletId);

            var wallet = await _context.Wallets
                .AsNoTracking()
                .Include(w => w.Currency)
                .Include(w => w.Holdings)
                .ThenInclude(w => w.Asset)
                .SingleOrDefaultAsync(w => w.Id == response.WalletId, cancellationToken);

            if (wallet != null)
            {
                _logger.LogDebug("Wallet {0} found, owned by user {1}.", wallet.Id, wallet.UserId);

                var tickers = await _tickerCache.GetTickersAsync(wallet.Holdings
                    .Select(h => new TickerPair(h.Asset.Symbol, wallet.Currency.Code))
                    .Distinct()
                    .ToArray()
                );

                foreach (var holding in wallet.Holdings)
                {
                    holding.Asset.CurrentValue = tickers
                        .Where(t => t.Pair.Left == holding.Asset.Symbol && t.Pair.Right == wallet.Currency.Code)
                        .Select(t => t.Value)
                        .FirstOrDefault();
                    // Avoids circular ref.
                    holding.Wallet = null;
                }

                await _hubContext.Clients
                    .User(wallet.UserId)
                    .SendAsync(nameof(IDashboardClient.WalletBalanceChanged), wallet, cancellationToken);
            }
            else
            {
                _logger.LogWarning("The wallet {0} does not exist.", response.WalletId);
            }

            return Unit.Value;
        }
    }
}
