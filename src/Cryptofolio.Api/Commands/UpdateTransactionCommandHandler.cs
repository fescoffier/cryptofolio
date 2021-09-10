using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Api.Commands
{
    /// <summary>
    /// Provides an <see cref="IRequestHandler{TRequest, TResponse}"/> implementation to handle <see cref="UpdateTransactionCommand"/>.
    /// </summary>
    public class UpdateTransactionCommandHandler : IRequestHandler<UpdateTransactionCommand, CommandResult>
    {
        private readonly CryptofolioContext _context;
        private readonly IEventDispatcher _dispatcher;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<UpdateTransactionCommandHandler> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="UpdateTransactionCommandHandler"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <param name="systemClock">The system clock.</param>
        /// <param name="logger">The logger.</param>
        public UpdateTransactionCommandHandler(
            CryptofolioContext context,
            IEventDispatcher dispatcher,
            ISystemClock systemClock,
            ILogger<UpdateTransactionCommandHandler> logger)
        {
            _context = context;
            _dispatcher = dispatcher;
            _systemClock = systemClock;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<CommandResult> Handle(UpdateTransactionCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling the {1} command.", command.RequestId);

            try
            {
                _logger.LogDebug("Beginning a new transaction.");
                using var dbTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                _logger.LogDebug("Transaction {0} just begun.", dbTransaction.TransactionId);

                var transaction = await UpdateEntity(command, cancellationToken);
                _logger.LogDebug("Storing the updated transaction in database.");
                await _context.SaveChangesAsync(cancellationToken);

                var @event = new TransactionUpdatedEvent
                {
                    Id = command.RequestId,
                    Date = _systemClock.UtcNow,
                    UserId = command.UserId,
                    Transaction = transaction
                };
                _logger.LogDebug("Dispatching a {0} event.", nameof(TransactionUpdatedEvent));
                await _dispatcher.DispatchAsync(@event);

                _logger.LogDebug("Committing the transaction {0}.", dbTransaction.TransactionId);
                await dbTransaction.CommitAsync(CancellationToken.None);

                _logger.LogInformation("Command {0} handled.", command.RequestId);

                return CommandResult.Success();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error has occured while handling the {1} command.", command.RequestId);
                return CommandResult.Failed(CommandConstants.Transaction.Errors.UpdateError);
            }
        }

        private async Task<Transaction> UpdateEntity(UpdateTransactionCommand command, CancellationToken cancellationToken)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Wallet).ThenInclude(t => t.Currency)
                .Include(t => t.Asset)
                .Include(t => t.Exchange)
                .Include(nameof(BuyOrSellTransaction.Currency))
                .SingleOrDefaultAsync(t => t.Id == command.Id, cancellationToken);

            if (transaction is BuyOrSellTransaction bst &&
                (command.Type == CommandConstants.Transaction.Types.Buy || command.Type == CommandConstants.Transaction.Types.Sell))
            {
                bst.Date = command.Date;
                bst.Exchange = await _context.Exchanges.SingleOrDefaultAsync(e => e.Id == command.ExchangeId, cancellationToken);
                bst.Currency = await _context.Currencies.SingleOrDefaultAsync(c => c.Id == command.CurrencyId, cancellationToken);
                bst.Price = command.Price;
                bst.Qty = command.Qty;
                bst.InitialValue = command.Qty * command.Price;
                bst.Type = command.Type;
                bst.Note = command.Note;
            }
            else if (transaction is TransferTransaction tft && command.Type == CommandConstants.Transaction.Types.Transfer)
            {
                tft.Date = command.Date;
                tft.Exchange = await _context.Exchanges.SingleOrDefaultAsync(e => e.Id == command.ExchangeId, cancellationToken);
                tft.Qty = command.Qty;
                tft.Source = command.Source;
                tft.Destination = command.Destination;
                tft.Note = command.Note;

                var ticker = await _context.AssetTickers
                        .AsNoTracking()
                        .Where(t => t.Asset.Id == tft.Asset.Id && t.VsCurrency.Id == tft.Wallet.Currency.Id && t.Timestamp <= command.Date)
                        .OrderByDescending(t => t.Timestamp)
                        .Select(t => t.Value)
                        .FirstOrDefaultAsync(cancellationToken);
                tft.InitialValue = tft.Qty * ticker;
            }
            else
            {
                throw new InvalidOperationException($"Unsupported transaction type: {command.Type}");
            }

            transaction.Date = command.Date;
            transaction.Exchange = await _context.Exchanges.SingleOrDefaultAsync(e => e.Id == command.ExchangeId, cancellationToken);
            transaction.Qty = command.Qty;
            transaction.Note = command.Note;

            return transaction;
        }
    }
}
