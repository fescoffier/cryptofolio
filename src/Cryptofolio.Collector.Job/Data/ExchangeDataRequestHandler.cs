using CoinGecko.Entities.Response.Exchanges;
using CoinGecko.Interfaces;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Data;
using Cryptofolio.Infrastructure.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Collector.Job.Data
{
    /// <summary>
    /// Provides an implementation of <see cref="IPipelineBehavior{TRequest, TResponse}"/> to handle <see cref="ExchangeDataRequest"/> message.
    /// </summary>
    public class ExchangeDataRequestHandler : IPipelineBehavior<ExchangeDataRequest, Unit>
    {
        private readonly CryptofolioContext _context;
        private readonly IExchangesClient _exchangesClient;
        private readonly IEventDispatcher _dispatcher;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<AssetDataRequestHandler> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="ExchangeDataRequestHandler"/>.
        /// </summary>
        /// <param name="context">The Db context.</param>
        /// <param name="exchangesClient">The exchanges client.</param>
        /// <param name="dispatcher">The event dispatcher.</param>
        /// <param name="systemClock">The system clock.</param>
        /// <param name="logger">The logger.</param>
        public ExchangeDataRequestHandler(
            CryptofolioContext context,
            IExchangesClient exchangesClient,
            IEventDispatcher dispatcher,
            ISystemClock systemClock,
            ILogger<AssetDataRequestHandler> logger)
        {
            _context = context;
            _exchangesClient = exchangesClient;
            _dispatcher = dispatcher;
            _systemClock = systemClock;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(ExchangeDataRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Handling exchange data request {0} submitted at {1} for the {2} exchange", request.TraceIdentifier, request.Date, request.Id);

            ExchangeById exchangeData;

            try
            {
                _logger.LogDebug("Fetching {0} exchange data from Coingecko.", request.Id);
                exchangeData = await _exchangesClient.GetExchangesByExchangeId(request.Id);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error has occured while fetching the {0} exchange data from Coingecko.", request.Id);
                return Unit.Value;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var exchange = await _context.Exchanges.SingleOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
            if (exchange == null)
            {
                exchange = new()
                {
                    Id = request.Id
                };
                _context.Exchanges.Add(exchange);
            }
            exchange.Name = exchangeData.Name;
            exchange.Description = exchangeData.Description;
            exchange.YearEstablished = exchangeData.YearEstablished;
            exchange.Url = exchangeData.Url;
            exchange.Image = exchangeData.Image;

            try
            {
                _logger.LogDebug("Saving changes.");
                var count = await _context.SaveChangesAsync(cancellationToken);
                _logger.LogDebug("Changes saved with {0} row(s) modified.", count);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error has occured while upserting the {0} exchange data.", request.Id);
                return Unit.Value;
            }

            _logger.LogDebug("Dispatching an {0} event.", nameof(ExchangeInfosUpsertedEvent));
            await _dispatcher.DispatchAsync(new ExchangeInfosUpsertedEvent
            {
                Id = Guid.NewGuid().ToString(),
                Date = _systemClock.UtcNow,
                Exchange = exchange
            });

            _logger.LogInformation("Exchange data request {0} submitted at {1} for the {2} exchange handled successfully.", request.TraceIdentifier, request.Date, request.Id);

            return Unit.Value;
        }
    }
}
