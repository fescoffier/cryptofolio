using CoinGecko.Entities.Response.Simple;
using CoinGecko.Interfaces;
using Cryptofolio.Core;
using Cryptofolio.Core.Entities;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Collector.Job.Data
{
    /// <summary>
    /// Provides an implementation of <see cref="IPipelineBehavior{AssetTickerDataRequest, Unit}"/> to handle <see cref="AssetTickerDataRequest"/> message.
    /// </summary>
    public class AssetTickerDataRequestHandler : IPipelineBehavior<AssetTickerDataRequest, Unit>
    {
        private readonly CryptofolioContext _context;
        private readonly ISimpleClient _simpleClient;
        private readonly IEventDispatcher _dispatcher;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<AssetTickerDataRequestHandler> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="AssetTickerDataRequestHandler"/>.
        /// </summary>
        /// <param name="context">The Db context.</param>
        /// <param name="simpleClient">The simple client.</param>
        /// <param name="dispatcher">The event dispatcher.</param>
        /// <param name="systemClock">The system clock.</param>
        /// <param name="logger">The logger.</param>
        public AssetTickerDataRequestHandler(
            CryptofolioContext context,
            ISimpleClient simpleClient,
            IEventDispatcher dispatcher,
            ISystemClock systemClock,
            ILogger<AssetTickerDataRequestHandler> logger)
        {
            _context = context;
            _simpleClient = simpleClient;
            _dispatcher = dispatcher;
            _systemClock = systemClock;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(AssetTickerDataRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            var ids = string.Join(',', request.Ids);
            var vsCurrencies = string.Join(',', request.VsCurrencies);

            _logger.LogInformation("Handling asset ticker data request {0} submitted at {1} for the [{2}] assets versus currencies [{3}].",
                request.TraceIdentifier,
                request.Date,
                ids,
                vsCurrencies);

            Price price;

            try
            {
                _logger.LogDebug("Fetching [{0}] coins price versus currencies [{1}] from Coingecko.", ids, vsCurrencies);
                price = await _simpleClient.GetSimplePrice(
                    request.Ids.ToArray(),
                    request.VsCurrencies.ToArray(),
                    false,
                    false,
                    false,
                    true
                );
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error has occured while fetching [{0}] coins price versus currencies [{1}] from Coingecko.", ids, vsCurrencies);
                return Unit.Value;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var updatedIds = new HashSet<string>();
            var updatedCurrencies = new HashSet<string>();
            foreach (var id in request.Ids)
            {
                var asset = await _context.Assets.SingleOrDefaultAsync(a => a.Id == id, cancellationToken);
                if (asset == null)
                {
                    _logger.LogError("The {0} asset does not exists.", id);
                }

                foreach (var currency in request.VsCurrencies)
                {
                    var tickerValue = price[id][currency].Value;
                    var tickerLastUpdatedAtMs = price[id]["last_updated_at"].Value;
                    var tickerLastUpdatedAt = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(tickerLastUpdatedAtMs));
                    var tickerExists = await _context.AssetTickers
                        .AnyAsync(t =>
                            t.Asset.Id == id &&
                            t.Timestamp == tickerLastUpdatedAt &&
                            t.VsCurrency == currency,
                            cancellationToken
                        );
                    if (!tickerExists)
                    {
                        _logger.LogDebug("Ticker at {0} for {1} versus currency {2} does not exists.", tickerLastUpdatedAt, id, currency);
                        _context.AssetTickers.Add(new AssetTicker
                        {
                            Asset = asset,
                            Timestamp = tickerLastUpdatedAt,
                            Value = tickerValue,
                            VsCurrency = currency
                        });
                        updatedIds.Add(id);
                        updatedCurrencies.Add(currency);
                    }
                    else
                    {
                        _logger.LogDebug("Ticker at {0} for {1} versus currency {2} exists.", tickerLastUpdatedAt, id, currency);
                    }
                }
            }

            try
            {
                _logger.LogDebug("Saving changes.");
                var count = await _context.SaveChangesAsync(cancellationToken);
                _logger.LogDebug("Changes saved with {0} row(s) modified.", count);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error has occured while upserting the tickers data.");
                return Unit.Value;
            }

            _logger.LogDebug("Dispatching an {0} event.", nameof(AssetTickerUpsertedEvent));
            await _dispatcher.DispatchAsync(new AssetTickerUpsertedEvent
            {
                Date = _systemClock.UtcNow,
                Ids = updatedIds,
                VsCurrencies = updatedCurrencies
            });

            _logger.LogInformation("Asset ticker data request {0} submitted at {1} for the [{2}] assets versus currencies [{3}] handled successfully.",
                request.TraceIdentifier,
                request.Date,
                ids,
                vsCurrencies);

            return Unit.Value;
        }
    }
}
