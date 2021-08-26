using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Entities;
using MediatR;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Api.Commands
{
    /// <summary>
    /// Provides an <see cref="IRequestHandler{TRequest, TResponse}"/> implementation to handle <see cref="UpdateWalletCommand"/>.
    /// </summary>
    public class UpdateWalletCommandHandler : IRequestHandler<UpdateWalletCommand, CommandResult>
    {
        private readonly CryptofolioContext _context;
        private readonly IEventDispatcher _dispatcher;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<UpdateWalletCommandHandler> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="CreateWalletCommandHandler"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <param name="systemClock">The system clock.</param>
        /// <param name="logger">The logger.</param>
        public UpdateWalletCommandHandler(
            CryptofolioContext context,
            IEventDispatcher dispatcher,
            ISystemClock systemClock,
            ILogger<UpdateWalletCommandHandler> logger)
        {
            _context = context;
            _dispatcher = dispatcher;
            _systemClock = systemClock;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<CommandResult> Handle(UpdateWalletCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling the {1} command.", command.RequestId);

            try
            {
                _logger.LogDebug("Beginning a new transaction.");
                using var dbTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                _logger.LogDebug("Transaction {0} just begun.", dbTransaction.TransactionId);

                var wallet = new Wallet
                {
                    Id = command.Id,
                    Name = command.Name,
                    Description = command.Description
                };
                var walletEntry = _context.Wallets.Attach(wallet);
                walletEntry.Property(p => p.Name).IsModified = true;
                walletEntry.Property(p => p.Description).IsModified = true;
                _logger.LogDebug("Storing the updated wallet in database.");
                await _context.SaveChangesAsync(cancellationToken);

                var @event = new WalletUpdatedEvent
                {
                    Id = command.RequestId,
                    Date = _systemClock.UtcNow,
                    UserId = command.UserId,
                    Wallet = wallet
                };
                _logger.LogDebug("Dispatching a {0} event.", nameof(WalletUpdatedEvent));
                await _dispatcher.DispatchAsync(@event);

                _logger.LogDebug("Committing the transaction {0}.", dbTransaction.TransactionId);
                await dbTransaction.CommitAsync(CancellationToken.None);

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
