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
    /// Provides an <see cref="IRequestHandler{TRequest, TResponse}"/> implementation to handle <see cref="CreateTransactionCommand"/>.
    /// </summary>
    public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, CommandResult<Transaction>>
    {
        private readonly CryptofolioContext _context;
        private readonly IEventDispatcher _dispatcher;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<CreateTransactionCommandHandler> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="CreateTransactionCommandHandler"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <param name="systemClock">The system clock.</param>
        /// <param name="logger">The logger.</param>
        public CreateTransactionCommandHandler(
            CryptofolioContext context,
            IEventDispatcher dispatcher,
            ISystemClock systemClock,
            ILogger<CreateTransactionCommandHandler> logger)
        {
            _context = context;
            _dispatcher = dispatcher;
            _systemClock = systemClock;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<CommandResult<Transaction>> Handle(CreateTransactionCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling the {1} command.", command.RequestId);

            try
            {
                _logger.LogDebug("Beginning a new transaction.");
                using var dbTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                _logger.LogDebug("Transaction {0} just begun.", dbTransaction.TransactionId);

                var transaction = await CreateEntity(command, cancellationToken);
                if (!await _context.Wallets.AnyAsync(w => w.Id == command.WalletId && w.UserId == command.UserId, cancellationToken))
                {
                    _logger.LogWarning("The wallet {0} doesn't exist for the user {1}.", command.WalletId, command.UserId);
                    return CommandResult<Transaction>.Failed(CommandConstants.Transaction.Errors.CreateInvalidWalletError);
                }

                _context.Transactions.Add(transaction);
                _logger.LogDebug("Storing the created transaction in database.");
                await _context.SaveChangesAsync(cancellationToken);

                var @event = new TransactionCreatedEvent
                {
                    Id = command.RequestId,
                    Date = _systemClock.UtcNow,
                    UserId = command.UserId,
                    Transaction = transaction
                };
                _logger.LogDebug("Dispatching a {0} event.", nameof(TransactionCreatedEvent));
                await _dispatcher.DispatchAsync(@event);

                _logger.LogDebug("Committing the transaction {0}.", dbTransaction.TransactionId);
                await dbTransaction.CommitAsync(CancellationToken.None);

                _logger.LogInformation("Command {0} handled.", command.RequestId);

                return CommandResult<Transaction>.Success(transaction);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error has occured while handling the {1} command.", command.RequestId);
                return CommandResult<Transaction>.Failed(CommandConstants.Transaction.Errors.CreateError);
            }
        }

        private async Task<Transaction> CreateEntity(CreateTransactionCommand command, CancellationToken cancellationToken)
        {
            if (command.Type == CommandConstants.Transaction.Types.Buy ||
                command.Type == CommandConstants.Transaction.Types.Sell)
            {
                return new BuyOrSellTransaction
                {
                    Id = Guid.NewGuid().ToString(),
                    Date = command.Date,
                    Wallet = await _context.Wallets.SingleOrDefaultAsync(w => w.Id == command.WalletId, cancellationToken),
                    Asset = await _context.Assets.SingleOrDefaultAsync(a => a.Id == command.AssetId, cancellationToken),
                    Exchange = await _context.Exchanges.SingleOrDefaultAsync(e => e.Id == command.ExchangeId, cancellationToken),
                    Currency = await _context.Currencies.SingleOrDefaultAsync(c => c.Id == command.CurrencyId, cancellationToken),
                    Price = command.Price,
                    Qty = command.Qty,
                    InitialValue = command.Qty * command.Price,
                    Type = command.Type,
                    Note = command.Note
                };
            }
            else if (command.Type == CommandConstants.Transaction.Types.Transfer)
            {
                return new TransferTransaction
                {
                    Id = Guid.NewGuid().ToString(),
                    Date = command.Date,
                    Wallet = await _context.Wallets.SingleOrDefaultAsync(w => w.Id == command.WalletId, cancellationToken),
                    Asset = await _context.Assets.SingleOrDefaultAsync(a => a.Id == command.AssetId, cancellationToken),
                    Exchange = await _context.Exchanges.SingleOrDefaultAsync(e => e.Id == command.ExchangeId, cancellationToken),
                    Qty = command.Qty,
                    Source = command.Source,
                    Destination = command.Destination,
                    Note = command.Note
                };
            }

            throw new InvalidOperationException($"Unsupported transaction type: {command.Type}");
        }
    }
}
