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
        public Task<Unit> Handle(TransactionCreatedEvent @event, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Handling the event occurence {0} of type {1}.", @event.Id, typeof(TransactionCreatedEvent).FullName);
            return ComputeHolding(@event.Transaction.Asset, @event.Transaction.Wallet, false, cancellationToken).ContinueWith(_ => Unit.Value);
        }

        /// <inheritdoc/>
        public Task<Unit> Handle(TransactionUpdatedEvent @event, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Handling the event occurence {0} of type {1}.", @event.Id, typeof(TransactionUpdatedEvent).FullName);
            return ComputeHolding(@event.Transaction.Asset, @event.Transaction.Wallet, false, cancellationToken).ContinueWith(_ => Unit.Value);
        }

        /// <inheritdoc/>
        public Task<Unit> Handle(TransactionDeletedEvent @event, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Handling the event occurence {0} of type {1}.", @event.Id, typeof(TransactionDeletedEvent).FullName);
            return ComputeHolding(@event.Transaction.Asset, @event.Transaction.Wallet, true, cancellationToken).ContinueWith(_ => Unit.Value);
        }

        private async Task ComputeHolding(Asset asset, Wallet wallet, bool delete, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Computing holding for the asset {0} in the wallet {1} using \"delete\": {0}",
                asset.Id,
                wallet.Id,
                delete
            );

            var holding = await _context.Holdings.SingleOrDefaultAsync(h => h.Asset.Id == asset.Id && h.Wallet.Id == wallet.Id, cancellationToken);
            if (holding == null && !delete)
            {
                _logger.LogDebug("The holding does not exist. Creating it.");
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
                _logger.LogDebug("The holding amount is equal to 0 after a transaction delete. Removing it.");
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
                _logger.LogTrace("Amount of {0} in the wallet {1}: {2}", asset.Id, wallet.Id, holding.Amount);
            }

            _logger.LogInformation("Holding computed for the asset {0} in the wallet {1}.", asset.Id, wallet.Id);

            await _context.SaveChangesAsync(cancellationToken);

            // TODO: Trigger wallet balance recompute.
        }
    }
}
