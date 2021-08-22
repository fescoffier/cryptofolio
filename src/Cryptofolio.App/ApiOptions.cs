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
        /// The wallets endpoint.
        /// </summary>
        public string WalletsEndpoint => $"{Url}/wallets";

        /// <summary>
        /// The transactions endpoint.
        /// </summary>
        public string TransactionsEndpoint => $"{Url}/transactions";
    }
}
