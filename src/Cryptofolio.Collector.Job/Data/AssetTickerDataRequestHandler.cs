using CoinGecko.Entities.Response.Coins;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Collector.Job.Data
{
    public class AssetTickerDataRequestHandler : IPipelineBehavior<AssetTickerDataRequest, Unit>
    {
        private readonly CryptofolioContext _context;
        private readonly ISimpleClient _simpleClient;
        private readonly IEventDispatcher _dispatcher;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<AssetTickerDataRequestHandler> _logger;

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
                    var tickerLastUpdatedAt = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(tickerLastUpdatedAtMs));
                    var tickerExists = await _context.AssetTickers
                        .AnyAsync(t =>
                            t.Asset.Id == id &&
                            t.Timestamp == tickerLastUpdatedAt &&
                            t.VsCurrency == currency,
                            cancellationToken
                        );
                    if (!tickerExists)
                    {
                        _context.AssetTickers.Add(new AssetTicker
                        {
                            Asset = asset,
                            Timestamp = tickerLastUpdatedAt,
                            Value = tickerValue,
                            VsCurrency = currency
                        });
                    }
                }
            }

            var asset = await _context.Assets.SingleOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
            if (asset == null)
            {
                asset = new()
                {
                    Id = request.Id
                };
                _context.Assets.Add(asset);
            }
            asset.Name = price.Name;
            asset.Symbol = price.Symbol;
            // TODO: Might be a good idea to handle different localization.
            asset.Description = price.Description["en"];

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
                Date = _systemClock.UtcNow,
                Asset = asset
            });

            _logger.LogInformation("Asset data request {0} submitted at {1} for the {2} asset handled successfully.", request.TraceIdentifier, request.Date, request.Id);

            return Unit.Value;
        }
    }
}
