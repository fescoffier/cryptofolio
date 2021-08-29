namespace Cryptofolio.Collector.Job.Fixer
{
    /// <summary>
    /// Models a <see cref="FixerClient"/> response base.
    /// </summary>
    public class FixerResponseBase
    {
        /// <summary>
        /// Defines if the API call has succeeded.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The error.
        /// </summary>
        public FixerResponseError Error { get; set; }
    }
}
