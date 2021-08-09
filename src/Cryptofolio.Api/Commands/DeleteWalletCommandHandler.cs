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
    /// Provides an <see cref="IRequestHandler{TRequest, TResponse}"/> implementation to handle <see cref="DeleteWalletCommandHandler"/>.
    /// </summary>
    public class DeleteWalletCommandHandler : IRequestHandler<DeleteWalletCommand, CommandResult>
    {
        private readonly CryptofolioContext _context;
        private readonly IEventDispatcher _dispatcher;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<DeleteWalletCommandHandler> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="CreateWalletCommandHandler"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <param name="systemClock">The system clock.</param>
        /// <param name="logger">The logger.</param>
        public DeleteWalletCommandHandler(
            CryptofolioContext context,
            IEventDispatcher dispatcher,
            ISystemClock systemClock,
            ILogger<DeleteWalletCommandHandler> logger)
        {
            _context = context;
            _dispatcher = dispatcher;
            _systemClock = systemClock;
            _logger = logger;
        }

        public async Task<CommandResult> Handle(DeleteWalletCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling the {1} command.", command.RequestId);

            try
            {
                _logger.LogDebug("Beginning a new transaction.");
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                _logger.LogDebug("Transaction {0} just begun.", transaction.TransactionId);

                var wallet = await _context.Wallets.SingleOrDefaultAsync(w => w.Id == command.Id, cancellationToken);
                _context.Wallets.Remove(wallet);
                _logger.LogDebug("Storing the updated wallet in database.");
                await _context.SaveChangesAsync(cancellationToken);

                var @event = new WalletDeletedEvent
                {
                    Id = command.RequestId,
                    Date = _systemClock.UtcNow,
                    UserId = command.UserId,
                    Wallet = wallet
                };
                _logger.LogDebug("Dispatching a {0} event.", nameof(WalletDeletedEvent));
                await _dispatcher.DispatchAsync(@event);

                _logger.LogDebug("Committing the transaction {0}.", transaction.TransactionId);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Command {0} handled.", command.RequestId);

                return CommandResult.Success();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error has occured while handling the {1} command.", command.RequestId);
                return CommandResult.Failed(CommandConstants.Wallet.Errors.DeleteError);
            }
        }
    }
}
