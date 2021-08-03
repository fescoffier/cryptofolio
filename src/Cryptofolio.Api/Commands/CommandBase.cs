using System;

namespace Cryptofolio.Api.Commands
{
    /// <summary>
    /// Models a command.
    /// </summary>
    public abstract class CommandBase
    {
        private RequestContext _requestContext;
        /// <summary>
        /// The request context.
        /// </summary>
        public RequestContext RequestContext
        {
            get => _requestContext;
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                _requestContext = value;
            }
        }

        /// <summary>
        /// The request id.
        /// </summary>
        public string RequestId => _requestContext?.RequestId;

        /// <summary>
        /// The user id.
        /// </summary>
        public string UserId => _requestContext?.UserId;
    }
}
