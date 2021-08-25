namespace Cryptofolio.App
{
    /// <summary>
    /// Configuration options for the API endpoints.
    /// </summary>
    public class ApiOptions
    {
        /// <summary>
        /// The API url.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The assets endpoint.
        /// </summary>
        public string AssetsEndpoint => $"{Url}/assets";

        /// <summary>
        /// The currencies endpoint.
        /// </summary>
        public string CurrenciesEndpoint => $"{Url}/currencies";

        /// <summary>
        /// The exchanges endpoint.
        /// </summary>
        public string ExchangesEndpoint => $"{Url}/exchanges";

        /// <summary>
        /// The transactions endpoint.
        /// </summary>
        public string TransactionsEndpoint => $"{Url}/transactions";

        /// <summary>
        /// The wallets endpoint.
        /// </summary>
        public string WalletsEndpoint => $"{Url}/wallets";

    }
}
