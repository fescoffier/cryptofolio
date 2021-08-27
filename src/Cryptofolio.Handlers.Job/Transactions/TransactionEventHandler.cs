using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Handlers.Job.Transactions
{
    /// <summary>
    /// Provides an implementation of <see cref="IPipelineBehavior{IEvent, Unit}"/> to handle events of types <see cref="TransactionCreatedEvent"/>, <see cref="TransactionUpdatedEvent"/> and <see cref="TransactionDeletedEvent"/>.
    /// It recomputes the holdings of a wallet.
    /// </summary>
    public class TransactionEventHandler :
        IPipelineBehavior<TransactionCreatedEvent, Unit>,
        IPipelineBehavior<TransactionUpdatedEvent, Unit>,
        IPipelineBehavior<TransactionDeletedEvent, Unit>
    {
        private readonly CryptofolioContext _context;
        private readonly ILogger<TransactionEventHandler> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="TransactionEventHandler"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="logger">The logger.</param>
        public TransactionEventHandler(CryptofolioContext context, ILogger<TransactionEventHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <inheritdoc/>
        public Task<Unit> Handle(TransactionCreatedEvent request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next) =>
            ComputeHolding(request.Transaction.Asset, request.Transaction.Wallet, false, cancellationToken).ContinueWith(_ => Unit.Value);

        /// <inheritdoc/>
        public Task<Unit> Handle(TransactionUpdatedEvent request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next) =>
            ComputeHolding(request.Transaction.Asset, request.Transaction.Wallet, false, cancellationToken).ContinueWith(_ => Unit.Value);

        /// <inheritdoc/>
        public Task<Unit> Handle(TransactionDeletedEvent request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next) =>
            ComputeHolding(request.Transaction.Asset, request.Transaction.Wallet, true, cancellationToken).ContinueWith(_ => Unit.Value);

        private async Task ComputeHolding(Asset asset, Wallet wallet, bool delete, CancellationToken cancellationToken)
        {
            var holding = await _context.Holdings.SingleOrDefaultAsync(h => h.Asset.Id == asset.Id && h.Wallet.Id == wallet.Id, cancellationToken);
            if (holding == null && !delete)
            {
                holding = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Asset = await _context.Assets.SingleOrDefaultAsync(a => a.Id == asset.Id, cancellationToken),
                    Wallet = await _context.Wallets.SingleOrDefaultAsync(w => w.Id == wallet.Id, cancellationToken)
                };
                _context.Holdings.Add(holding);
            }

            var transactions = await _context.Transactions
                .AsNoTracking()
                .Where(t => t.Wallet.Id == wallet.Id && t.Asset.Id == asset.Id)
                .ToListAsync(cancellationToken);
            if (transactions.Count == 0 && delete)
            {
                _context.Holdings.Remove(holding);
            }
            else
            {
                holding.Amount = transactions.Sum(t =>
                {
                    if (t is BuyOrSellTransaction buyOrSellTransaction)
                    {
                        return buyOrSellTransaction.Type == InfrastructureConstants.Transactions.Types.Buy
                            ? buyOrSellTransaction.Qty
                            : buyOrSellTransaction.Qty * -1;
                    }
                    else if (t is TransferTransaction transferTransaction)
                    {
                        return transferTransaction.Destination == InfrastructureConstants.Transactions.Destinations.MyWallet
                            ? transferTransaction.Qty
                            : transferTransaction.Qty * -1;
                    }
                    return 0m;
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
