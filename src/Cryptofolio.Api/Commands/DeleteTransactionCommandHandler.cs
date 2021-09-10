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
    /// Provides an <see cref="IRequestHandler{TRequest, TResponse}"/> implementation to handle <see cref="DeleteTransactionCommand"/>.
    /// </summary>
    public class DeleteTransactionCommandHandler : IRequestHandler<DeleteTransactionCommand, CommandResult>
    {
        private readonly CryptofolioContext _context;
        private readonly IEventDispatcher _dispatcher;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<DeleteTransactionCommandHandler> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="UpdateTransactionCommandHandler"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <param name="systemClock">The system clock.</param>
        /// <param name="logger">The logger.</param>
        public DeleteTransactionCommandHandler(
            CryptofolioContext context,
            IEventDispatcher dispatcher,
            ISystemClock systemClock,
            ILogger<DeleteTransactionCommandHandler> logger)
        {
            _context = context;
            _dispatcher = dispatcher;
            _systemClock = systemClock;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<CommandResult> Handle(DeleteTransactionCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling the {1} command.", command.RequestId);

            try
            {
                _logger.LogDebug("Beginning a new transaction.");
                using var dbTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                _logger.LogDebug("Transaction {0} just begun.", dbTransaction.TransactionId);

                var transaction = await _context.Transactions
                    .Include(t => t.Wallet).ThenInclude(t => t.Currency)
                    .Include(t => t.Asset)
                    .Include(t => t.Exchange)
                    .Include(nameof(BuyOrSellTransaction.Currency))
                    .SingleOrDefaultAsync(t => t.Id == command.Id, cancellationToken);
                if (transaction.Wallet.UserId != command.UserId)
                {
                    _logger.LogWarning("This transaction isn't owned by the user {1}.", command.UserId);
                    return CommandResult.Failed(CommandConstants.Transaction.Errors.DeleteError);
                }

                _context.Transactions.Remove(transaction);
                _logger.LogDebug("Deleting the transaction in database.");
                await _context.SaveChangesAsync(cancellationToken);

                var @event = new TransactionDeletedEvent
                {
                    Id = command.RequestId,
                    Date = _systemClock.UtcNow,
                    UserId = command.UserId,
                    Transaction = transaction
                };
                _logger.LogDebug("Dispatching a {0} event.", nameof(TransactionDeletedEvent));
                await _dispatcher.DispatchAsync(@event);

                _logger.LogDebug("Committing the transaction {0}.", dbTransaction.TransactionId);
                await dbTransaction.CommitAsync(CancellationToken.None);

                _logger.LogInformation("Command {0} handled.", command.RequestId);

                return CommandResult.Success();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error has occured while handling the {1} command.", command.RequestId);
                return CommandResult.Failed(CommandConstants.Transaction.Errors.DeleteError);
            }
        }
    }
}
