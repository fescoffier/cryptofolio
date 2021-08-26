using MediatR;

namespace Cryptofolio.Api.Commands
{
    /// <summary>
    /// Models a command to delete a <see cref="Infrastructure.Entities.Transaction"/>.
    /// </summary>
    public class DeleteTransactionCommand : CommandBase, IRequest<CommandResult>
    {
        /// <summary>
        /// The id.
        /// </summary>
        public string Id { get; set; }
    }
}
