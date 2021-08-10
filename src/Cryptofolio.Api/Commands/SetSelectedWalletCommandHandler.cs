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
    /// Provides an <see cref="IRequestHandler{TRequest, TResponse}"/> implementation to handle <see cref="SetSelectedWalletCommand"/>.
    /// </summary>
    public class SetSelectedWalletCommandHandler : IRequestHandler<SetSelectedWalletCommand, CommandResult>
    {
        private readonly CryptofolioContext _context;
        private readonly IEventDispatcher _dispatcher;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<SetSelectedWalletCommandHandler> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="CreateWalletCommandHandler"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <param name="systemClock">The system clock.</param>
        /// <param name="logger">The logger.</param>
        public SetSelectedWalletCommandHandler(
            CryptofolioContext context,
            IEventDispatcher dispatcher,
            ISystemClock systemClock,
            ILogger<SetSelectedWalletCommandHandler> logger)
        {
            _context = context;
            _dispatcher = dispatcher;
            _systemClock = systemClock;
            _logger = logger;
        }

        public async Task<CommandResult> Handle(SetSelectedWalletCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling the {1} command.", command.RequestId);

            try
            {
                _logger.LogDebug("Beginning a new transaction.");
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                _logger.LogDebug("Transaction {0} just begun.", transaction.TransactionId);

                _logger.LogDebug("Updating the unselected wallet in database.");
                await _context.Database.ExecuteSqlInterpolatedAsync($"update \"data\".\"wallet\" set selected = false where user_id = {command.UserId}", cancellationToken: cancellationToken);
                _logger.LogDebug("Updating the selected wallet in database.");
                var rowsAffected = await _context.Database.ExecuteSqlInterpolatedAsync($"update \"data\".\"wallet\" set selected = true where user_id = {command.UserId} and id = {command.Id}", cancellationToken: cancellationToken);
                if (rowsAffected != 1)
                {
                    throw new InvalidOperationException("The number of updated rows should be exactly one.");
                }

                var @event = new WalletSelectedEvent
                {
                    Id = command.RequestId,
                    Date = _systemClock.UtcNow,
                    UserId = command.UserId,
                    Wallet = await _context.Wallets.AsNoTracking().SingleOrDefaultAsync(w => w.Id == command.Id, cancellationToken)
                };
                _logger.LogDebug("Dispatching a {0} event.", nameof(WalletSelectedEvent));
                await _dispatcher.DispatchAsync(@event);

                _logger.LogDebug("Committing the transaction {0}.", transaction.TransactionId);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Command {0} handled.", command.RequestId);

                return CommandResult.Success();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error has occured while handling the {1} command.", command.RequestId);
                return CommandResult.Failed(CommandConstants.Wallet.Errors.UpdateError);
            }
        }
    }
}
