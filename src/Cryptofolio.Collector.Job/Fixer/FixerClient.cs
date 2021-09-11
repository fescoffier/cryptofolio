using Cryptofolio.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Collector.Job.Fixer
{
    /// <summary>
    /// Provides a client to interact with the Fixer API.
    /// </summary>
    public class FixerClient
    {
        private readonly HttpClient _httpClient;
        private readonly IOptionsSnapshot<FixerOptions> _optionSnapshot;
        private readonly ILogger<FixerClient> _logger;

        private FixerOptions Options => _optionSnapshot.Value;

        /// <summary>
        /// Creates a new instance of <see cref="FixerClient"/>.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="optionSnapshot">The options snapshot.</param>
        /// <param name="logger">The logger.</param>
        public FixerClient(HttpClient httpClient, IOptionsSnapshot<FixerOptions> optionSnapshot, ILogger<FixerClient> logger)
        {
            _httpClient = httpClient;
            _optionSnapshot = optionSnapshot;
            _logger = logger;
        }

        /// <summary>
        /// Fetches the latest rates for the specified currency, against the specified currencies.
        /// </summary>
        /// <param name="currencyCode">The currency code to fetch the rates</param>
        /// <param name="versusCurrenciesCodes">The versus currencies codes to fetch the rates against.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<FixerLatestRatesResponse> GetLatestRatesAsync(
            string currencyCode,
            IEnumerable<string> versusCurrenciesCodes,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Fetching the latest rates for the {0} currency, against {1} currencies.",
                currencyCode,
                string.Join(',', versusCurrenciesCodes)
            );

            try
            {
                var requestUri = $"{Options.LatestEndpoint}?access_key={Options.ApiKey}&base={currencyCode}&symbols={string.Join(',', versusCurrenciesCodes)}";
                var response = await _httpClient.GetAsync(requestUri, cancellationToken);
                var rates = await JsonSerializer.DeserializeAsync<FixerLatestRatesResponse>(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    Options.SerializerOptions,
                    cancellationToken
                );
                _logger.LogTraceObject("Rates", rates);
                return rates;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error has occured while interacting with the Fixer API.");
            }

            return null;
        }
    }
}
