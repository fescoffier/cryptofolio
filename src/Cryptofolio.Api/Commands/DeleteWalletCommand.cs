using MediatR;

namespace Cryptofolio.Api.Commands
{
    /// <summary>
    /// Models a command to delete a <see cref="Infrastructure.Entities.Wallet"/>.
    /// </summary>
    public class DeleteWalletCommand : CommandBase, IRequest<CommandResult>
    {
        /// <summary>
        /// The id.
        /// </summary>
        public string Id { get; set; }
    }
}
