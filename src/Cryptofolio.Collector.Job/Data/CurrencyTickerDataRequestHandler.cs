using Cryptofolio.Collector.Job.Fixer;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Data;
using Cryptofolio.Infrastructure.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Collector.Job.Data
{
    /// <summary>
    /// Provides an implementation of <see cref="IPipelineBehavior{TRequest, TResponse}"/> to handle <see cref="CurrencyTickerDataRequest"/> message.
    /// </summary>
    public class CurrencyTickerDataRequestHandler : IPipelineBehavior<CurrencyTickerDataRequest, Unit>
    {
        private readonly CryptofolioContext _context;
        private readonly FixerClient _fixerClient;
        private readonly IEventDispatcher _dispatcher;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<CurrencyTickerDataRequestHandler> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="CurrencyTickerDataRequestHandler"/>.
        /// </summary>
        /// <param name="context">The Db context.</param>
        /// <param name="fixerClient">The coins client.</param>
        /// <param name="dispatcher">The event dispatcher.</param>
        /// <param name="systemClock">The system clock.</param>
        /// <param name="logger">The logger.</param>
        public CurrencyTickerDataRequestHandler(
            CryptofolioContext context,
            FixerClient fixerClient,
            IEventDispatcher dispatcher,
            ISystemClock systemClock,
            ILogger<CurrencyTickerDataRequestHandler> logger)
        {
            _context = context;
            _fixerClient = fixerClient;
            _dispatcher = dispatcher;
            _systemClock = systemClock;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(CurrencyTickerDataRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Handling currency ticker data request {0} submitted at {1} for the {2} currency versus currencies [{3}].",
                request.TraceIdentifier,
                request.Date,
                request.Currency,
                request.VsCurrencies);

            var currency = await _context.Currencies.SingleOrDefaultAsync(a => a.Code == request.Currency, cancellationToken);
            if (currency == null)
            {
                _logger.LogError("The {0} currency does not exist.", request.Currency);
                return Unit.Value;
            }

            FixerLatestRatesResponse ratesResponse;

            try
            {
                _logger.LogDebug("Fetching {0} currency rates versus currencies [{1}] from Fixer.", request.Currency, request.VsCurrencies);
                ratesResponse = await _fixerClient.GetLatestRatesAsync(
                    request.Currency,
                    request.VsCurrencies,
                    cancellationToken
                );
            }
            catch (Exception e)
            {
                _logger.LogError(
                    e,
                    "An error has occured while fetching {0} currency rates versus currencies [{1}] from Fixer.",
                    request.Currency,
                    request.VsCurrencies
                );
                return Unit.Value;
            }

            var vsCurrencies = await _context.Currencies
                .Where(c => request.VsCurrencies.Contains(c.Code))
                .ToListAsync(cancellationToken);
            foreach (var unsupportedVsCurrency in vsCurrencies.Where(c => !ratesResponse.Rates.ContainsKey(c.Code.ToUpperInvariant())).ToArray())
            {
                _logger.LogWarning("Fixer did non return a ticker for the currency {0}.", unsupportedVsCurrency);
                vsCurrencies.Remove(unsupportedVsCurrency);
            }

            cancellationToken.ThrowIfCancellationRequested();

            foreach (var vsCurrency in vsCurrencies)
            {
                var tickerValue = ratesResponse.Rates[vsCurrency.Code.ToUpperInvariant()];
                var tickerExists = await _context.CurrencyTickers
                    .AnyAsync(t =>
                        t.Currency.Id == currency.Id &&
                        t.VsCurrency.Id == vsCurrency.Id &&
                        t.Timestamp == ratesResponse.Timestamp,
                        cancellationToken
                    );
                if (!tickerExists)
                {
                    _logger.LogDebug("Ticker at {0} for {1} versus currency {2} does not exists.", ratesResponse.Timestamp, currency.Code, vsCurrency.Code);
                    _context.CurrencyTickers.Add(new()
                    {
                        Currency = currency,
                        VsCurrency = vsCurrency,
                        Timestamp = ratesResponse.Timestamp,
                        Value = tickerValue
                    });
                }
                else
                {
                    _logger.LogDebug("Ticker at {0} for {1} versus currency {2} exists.", ratesResponse.Timestamp, currency.Code, vsCurrency.Code);
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

            _logger.LogDebug("Dispatching an {0} event.", nameof(CurrencyTickerUpsertedEvent));
            await _dispatcher.DispatchAsync(new CurrencyTickerUpsertedEvent
            {
                Id = Guid.NewGuid().ToString(),
                Date = _systemClock.UtcNow,
                Currency = currency,
                VsCurrencies = vsCurrencies
            });

            _logger.LogInformation("Currency ticker data request {0} submitted at {1} for the {2} currency versus currencies [{3}] handled successfully.",
                request.TraceIdentifier,
                request.Date,
                currency,
                vsCurrencies);

            return Unit.Value;
        }
    }
}
