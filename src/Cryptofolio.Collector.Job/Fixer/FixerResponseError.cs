namespace Cryptofolio.Collector.Job.Fixer
{
    /// <summary>
    /// Models a <see cref="FixerResponseBase"/> error.
    /// </summary>
    public class FixerResponseError
    {
        /// <summary>
        /// The error code.
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// The error info.
        /// </summary>
        public string Info { get; set; }
    }
}
