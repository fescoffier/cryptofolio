using Cryptofolio.Infrastructure.Entities;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

        /// <summary>
        /// The currency of the wallet.
        /// </summary>
        [Required(ErrorMessage = "The currency id is required")]
        [StringLength(36, ErrorMessage = "The currency id can't be greater than {0} characters")]
        [JsonPropertyName("currency_id")]
        public string CurrencyId { get; set; }
    }
}
