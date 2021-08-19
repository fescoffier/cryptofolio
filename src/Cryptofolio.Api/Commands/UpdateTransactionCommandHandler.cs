using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using System;
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

                var transaction = await UpdateEntity(command);
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
                await dbTransaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Command {0} handled.", command.RequestId);

                return CommandResult.Success();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error has occured while handling the {1} command.", command.RequestId);
                return CommandResult.Failed(CommandConstants.Transaction.Errors.UpdateError);
            }
        }

        private async Task<Transaction> UpdateEntity(UpdateTransactionCommand command)
        {
            Transaction transaction;

            if (command.Type == CommandConstants.Transaction.Types.Buy ||
                command.Type == CommandConstants.Transaction.Types.Sell)
            {
                var buyOrSellTransaction = new BuyOrSellTransaction
                {
                    Id = command.Id,
                    Date = command.Date,
                    Exchange = await _context.Exchanges.SingleOrDefaultAsync(e => e.Id == command.ExchangeId),
                    Currency = command.Currency,
                    Price = command.Price,
                    Qty = command.Qty,
                    Type = command.Type,
                    Note = command.Note
                };
                var buyOrSellTransactionEntry = _context.Attach(buyOrSellTransaction);
                buyOrSellTransactionEntry.Property(p => p.Type).IsModified = true;
                buyOrSellTransactionEntry.Property(p => p.Currency).IsModified = true;
                buyOrSellTransactionEntry.Property(p => p.Price).IsModified = true;
                transaction = buyOrSellTransaction;
            }
            else if (command.Type == CommandConstants.Transaction.Types.Transfer)
            {
                var transferTransaction = new TransferTransaction
                {
                    Id = command.Id,
                    Date = command.Date,
                    Exchange = await _context.Exchanges.SingleOrDefaultAsync(e => e.Id == command.ExchangeId),
                    Qty = command.Qty,
                    Source = command.Source,
                    Destination = command.Destination,
                    Note = command.Note
                };
                var transferTransactionEntry = _context.Attach(transferTransaction);
                transferTransactionEntry.Property(p => p.Source).IsModified = true;
                transferTransactionEntry.Property(p => p.Destination).IsModified = true;
                transaction = transferTransaction;
            }
            else
            {
                throw new InvalidOperationException($"Unsupported transaction type: {command.Type}");
            }

            var transactionEntry = _context.Entry(transaction);
            transactionEntry.Property(p => p.Date).IsModified = true;
            transactionEntry.Reference(p => p.Exchange).IsModified = true;
            transactionEntry.Property(p => p.Qty).IsModified = true;
            transactionEntry.Property(p => p.Note).IsModified = true;

            return transaction;
        }
    }
}
