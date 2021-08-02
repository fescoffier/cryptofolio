using Cryptofolio.Infrastructure.Entities;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Cryptofolio.Api.Commands
{
    /// <summary>
    /// Models a command to create <see cref="Wallet"/>.
    /// </summary>
    public class CreateWalletCommand : CommandBase, IRequest<CommandResult<Wallet>>
    {
        /// <summary>
        /// The name.
        /// </summary>
        [Required(ErrorMessage = "The name is required")]
        [StringLength(250, ErrorMessage = "The name can't have more than {1} characters")]
        public string Name { get; set; }

        /// <summary>
        /// The description.
        /// </summary>
        public string Description { get; set; }
    }
}
