using CoinGecko.Entities.Response.Simple;
using CoinGecko.Interfaces;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Data;
using Cryptofolio.Infrastructure.Entities;
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
    /// Provides an implementation of <see cref="IPipelineBehavior{TRequest, TResponse}"/> to handle <see cref="AssetTickerDataRequest"/> message.
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
            _logger.LogInformation("Handling asset ticker data request {0} submitted at {1} for the [{2}] assets versus currencies [{3}].",
                request.TraceIdentifier,
                request.Date,
                string.Join(',', request.Ids),
                string.Join(',', request.VsCurrencies));

            Price price;

            try
            {
                _logger.LogDebug("Fetching [{0}] coins price versus currencies [{1}] from Coingecko.",
                    string.Join(',', request.Ids),
                    string.Join(',', request.VsCurrencies));
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
                _logger.LogError(e, "An error has occured while fetching [{0}] coins price versus currencies [{1}] from Coingecko.",
                    string.Join(',', request.Ids),
                    string.Join(',', request.VsCurrencies));
                return Unit.Value;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var assets = await _context.Assets
                .Where(a => request.Ids.Contains(a.Id))
                .ToListAsync(cancellationToken);
            var vsCurrencies = await _context.Currencies
                .Where(c => request.VsCurrencies.Contains(c.Code))
                .ToListAsync(cancellationToken);
            var tickers = new List<AssetTicker>();
            foreach (var id in request.Ids)
            {
                var asset = assets.SingleOrDefault(a => a.Id == id);
                if (asset == null)
                {
                    _logger.LogError("The {0} asset does not exist.", id);
                    continue;
                }

                foreach (var code in request.VsCurrencies)
                {
                    var vsCurrency = vsCurrencies.SingleOrDefault(c => c.Code == code);
                    if (vsCurrency == null)
                    {
                        _logger.LogError("The {0} currency does not exist.", code);
                        continue;
                    }

                    var tickerValue = price[id][vsCurrency.Code].Value;
                    var tickerLastUpdatedAtSeconds = price[id]["last_updated_at"].Value;
                    var tickerLastUpdatedAt = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(tickerLastUpdatedAtSeconds));
                    var tickerExists = await _context.AssetTickers
                        .AnyAsync(t =>
                            t.Asset.Id == id &&
                            t.VsCurrency.Id == vsCurrency.Id &&
                            t.Timestamp == tickerLastUpdatedAt,
                            cancellationToken
                        );
                    if (!tickerExists)
                    {
                        _logger.LogDebug("Ticker at {0} for {1} versus currency {2} does not exists.", tickerLastUpdatedAt, id, vsCurrency);
                        tickers.Add(new()
                        {
                            Asset = asset,
                            Timestamp = tickerLastUpdatedAt,
                            Value = tickerValue,
                            VsCurrency = vsCurrency
                        });
                    }
                    else
                    {
                        _logger.LogDebug("Ticker at {0} for {1} versus currency {2} exists.", tickerLastUpdatedAt, id, vsCurrency);
                    }
                }
            }

            try
            {
                _logger.LogDebug("Saving changes.");
                _context.AssetTickers.AddRange(tickers);
                var count = await _context.SaveChangesAsync(cancellationToken);
                _logger.LogDebug("Changes saved with {0} row(s) modified.", count);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error has occured while upserting the tickers data.");
                return Unit.Value;
            }

            _logger.LogDebug("Dispatching an {0} event.", nameof(AssetTickersUpsertedEvent));
            await _dispatcher.DispatchAsync(new AssetTickersUpsertedEvent
            {
                Id = Guid.NewGuid().ToString(),
                Date = _systemClock.UtcNow,
                Tickers = tickers
            });

            _logger.LogInformation("Asset ticker data request {0} submitted at {1} for the [{2}] assets versus currencies [{3}] handled successfully.",
                request.TraceIdentifier,
                request.Date,
                string.Join(',', request.Ids),
                string.Join(',', request.VsCurrencies));

            return Unit.Value;
        }
    }
}
