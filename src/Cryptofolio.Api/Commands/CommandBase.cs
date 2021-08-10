namespace Cryptofolio.Api.Commands
{
    /// <summary>
    /// Models a command.
    /// </summary>
    public abstract class CommandBase
    {
        /// <summary>
        /// The request context.
        /// </summary>
        public RequestContext RequestContext { get; set; }

        /// <summary>
        /// The request id.
        /// </summary>
        public string RequestId => RequestContext?.RequestId;

        /// <summary>
        /// The user id.
        /// </summary>
        public string UserId => RequestContext?.UserId;
    }
}
