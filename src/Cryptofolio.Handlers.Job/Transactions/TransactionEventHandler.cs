using Confluent.Kafka;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Balances;
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
        private readonly KafkaProducerWrapper<string, ComputeWalletBalanceRequest> _producerWrapper;
        private readonly ILogger<TransactionEventHandler> _logger;

        private IProducer<string, ComputeWalletBalanceRequest> Producer => _producerWrapper.Producer;

        private KafkaProducerOptions<ComputeWalletBalanceRequest> ProducerOptions => _producerWrapper.Options;

        /// <summary>
        /// Creates a new instance of <see cref="TransactionEventHandler"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="logger">The logger.</param>
        public TransactionEventHandler(
            CryptofolioContext context,
            KafkaProducerWrapper<string, ComputeWalletBalanceRequest> producerWrapper,
            ILogger<TransactionEventHandler> logger)
        {
            _context = context;
            _producerWrapper = producerWrapper;
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
                .Include(nameof(BuyOrSellTransaction.Currency))
                .Where(t => t.Wallet.Id == wallet.Id && t.Asset.Id == asset.Id)
                .ToListAsync(cancellationToken);
            if (transactions.Count == 0 && delete)
            {
                _logger.LogDebug("The holding amount is equal to 0 after a transaction delete. Removing it.");
                _context.Holdings.Remove(holding);
            }
            else
            {
                holding.InitialValue = 0;
                holding.Qty = transactions.Sum(t =>
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
                if (holding.Qty < 0)
                {
                    holding.Qty = 0;
                }
                else if (holding.Qty > 0)
                {
                    foreach (var transaction in transactions)
                    {
                        if (transaction is BuyOrSellTransaction bst)
                        {
                            var rate = 1m;
                            if (bst.Currency.Id != wallet.Currency.Id)
                            {
                                rate = await _context.CurrencyTickers
                                    .AsNoTracking()
                                    .Where(t => t.Currency.Id == bst.Currency.Id && t.VsCurrency.Id == wallet.Currency.Id && t.Timestamp <= bst.Date)
                                    .OrderByDescending(t => t.Timestamp)
                                    .Select(t => t.Value)
                                    .FirstOrDefaultAsync(cancellationToken);
                            }

                            if (bst.Type == InfrastructureConstants.Transactions.Types.Buy)
                            {
                                holding.InitialValue += (bst.InitialValue * rate);
                            }
                            else
                            {
                                holding.InitialValue -= (bst.InitialValue * rate);
                            }
                        }
                        else if (transaction is TransferTransaction tft)
                        {
                            if (tft.Destination == InfrastructureConstants.Transactions.Destinations.MyWallet)
                            {
                                holding.InitialValue += tft.InitialValue;
                            }
                        }
                    }
                }

                _logger.LogTrace("Quantity of {0} in the wallet {1}: {2}", asset.Id, wallet.Id, holding.Qty);
                _logger.LogTrace("Initial value of {0} in the wallet {1}: {2}", asset.Id, wallet.Id, holding.InitialValue);
            }

            wallet.InitialValue = holding.InitialValue +
                await _context.Holdings
                    .AsNoTracking()
                    .Where(h => h.Wallet.Id == wallet.Id && h.Id != holding.Id)
                    .SumAsync(h => h.InitialValue, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Holding computed for the asset {0} in the wallet {1}.", asset.Id, wallet.Id);
            _logger.LogInformation("Triggering a balance recompute of the wallet {0}.", wallet.Id);

            await Producer.ProduceAsync(
                ProducerOptions.Topic,
                new()
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = new()
                    {
                        WalletId = wallet.Id
                    }
                }
            );
        }
    }
}
