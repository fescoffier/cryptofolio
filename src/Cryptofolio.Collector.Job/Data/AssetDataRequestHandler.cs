using CoinGecko.Entities.Response.Coins;
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
    /// Provides an implementation of <see cref="IPipelineBehavior{TRequest, TResponse}"/> to handle <see cref="AssetDataRequest"/> message.
    /// </summary>
    public class AssetDataRequestHandler : IPipelineBehavior<AssetDataRequest, Unit>
    {
        private readonly CryptofolioContext _context;
        private readonly ICoinsClient _coinsClient;
        private readonly IEventDispatcher _dispatcher;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<AssetDataRequestHandler> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="AssetDataRequestHandler"/>.
        /// </summary>
        /// <param name="context">The Db context.</param>
        /// <param name="coinsClient">The coins client.</param>
        /// <param name="dispatcher">The event dispatcher.</param>
        /// <param name="systemClock">The system clock.</param>
        /// <param name="logger">The logger.</param>
        public AssetDataRequestHandler(
            CryptofolioContext context,
            ICoinsClient coinsClient,
            IEventDispatcher dispatcher,
            ISystemClock systemClock,
            ILogger<AssetDataRequestHandler> logger)
        {
            _context = context;
            _coinsClient = coinsClient;
            _dispatcher = dispatcher;
            _systemClock = systemClock;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(AssetDataRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Handling asset data request {0} submitted at {1} for the {2} asset", request.TraceIdentifier, request.Date, request.Id);

            CoinFullDataById coin;

            try
            {
                _logger.LogDebug("Fetching {0} coin data from Coingecko.", request.Id);
                coin = await _coinsClient.GetAllCoinDataWithId(request.Id);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error has occured while fetching the {0} coin data from Coingecko.", request.Id);
                return Unit.Value;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var asset = await _context.Assets.SingleOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
            if (asset == null)
            {
                asset = new()
                {
                    Id = request.Id
                };
                _context.Assets.Add(asset);
            }
            asset.Name = coin.Name;
            asset.Symbol = coin.Symbol;
            asset.Description = coin.Description["en"];

            try
            {
                _logger.LogDebug("Saving changes.");
                var count = await _context.SaveChangesAsync(cancellationToken);
                _logger.LogDebug("Changes saved with {0} row(s) modified.", count);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error has occured while upserting the {0} asset data.", request.Id);
                return Unit.Value;
            }

            _logger.LogDebug("Dispatching an {0} event.", nameof(AssetInfosUpsertedEvent));
            await _dispatcher.DispatchAsync(new AssetInfosUpsertedEvent
            {
                Id = Guid.NewGuid().ToString(),
                Date = _systemClock.UtcNow,
                Asset = asset
            });

            _logger.LogInformation("Asset data request {0} submitted at {1} for the {2} asset handled successfully.", request.TraceIdentifier, request.Date, request.Id);

            return Unit.Value;
        }
    }
}
