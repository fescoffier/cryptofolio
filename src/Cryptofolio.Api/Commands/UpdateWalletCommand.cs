using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Cryptofolio.Api.Commands
{
    /// <summary>
    /// Models a command to update a <see cref="Infrastructure.Entities.Wallet"/>
    /// </summary>
    public class UpdateWalletCommand : CommandBase, IRequest<CommandResult>
    {
        /// <summary>
        /// The id.
        /// </summary>
        [Required(ErrorMessage = "The wallet id is required")]
        public string Id { get; set; }

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
