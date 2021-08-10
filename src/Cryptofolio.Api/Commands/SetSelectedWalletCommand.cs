using MediatR;

namespace Cryptofolio.Api.Commands
{
    /// <summary>
    /// Models a command to define a <see cref="Infrastructure.Entities.Wallet"/> as the user selected.
    /// </summary>
    public class SetSelectedWalletCommand : CommandBase, IRequest<CommandResult>
    {
        /// <summary>
        /// The id.
        /// </summary>
        public string Id { get; set; }
    }
}
