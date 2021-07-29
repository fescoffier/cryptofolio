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
    /// Provides an <see cref="IRequestHandler{TRequest, TResponse}"/> implementation to handle <see cref="CreateWalletCommand"/>.
    /// </summary>
    public class CreateWalletCommandHandler : IRequestHandler<CreateWalletCommand, CommandResult<Wallet>>
    {
        private readonly CryptofolioContext _context;
        private readonly IEventDispatcher _dispatcher;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<CreateWalletCommand> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="CreateWalletCommandHandler"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <param name="systemClock">The system clock.</param>
        /// <param name="logger">The logger.</param>
        public CreateWalletCommandHandler(
            CryptofolioContext context,
            IEventDispatcher dispatcher,
            ISystemClock systemClock,
            ILogger<CreateWalletCommand> logger)
        {
            _context = context;
            _dispatcher = dispatcher;
            _systemClock = systemClock;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<CommandResult<Wallet>> Handle(CreateWalletCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling the {1} command.", request.RequestId);

            try
            {
                _logger.LogDebug("Beginning a new transaction.");
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                _logger.LogDebug("Transaction {0} just begun.", transaction.TransactionId);

                var wallet = new Wallet
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = request.Name,
                    Description = request.Description,
                    UserId = request.UserId
                };
                _context.Wallets.Add(wallet);
                _logger.LogDebug("Storing the created wallet in database.");
                await _context.SaveChangesAsync(cancellationToken);

                var @event = new WalletCreatedEvent
                {
                    Id = request.RequestId,
                    Date = _systemClock.UtcNow,
                    UserId = request.UserId,
                    Wallet = wallet
                };
                _logger.LogDebug("Dispatching a {0} event.", nameof(WalletCreatedEvent));
                await _dispatcher.DispatchAsync(@event);

                _logger.LogDebug("Committing the transaction {0}.", transaction.TransactionId);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Command {0} handled.", request.RequestId);

                return CommandResult<Wallet>.Success(wallet);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error has occured while handling the {1} command.", request.RequestId);
                // TODO: Define errors.
                return CommandResult<Wallet>.Failed();
            }
        }
    }
}
