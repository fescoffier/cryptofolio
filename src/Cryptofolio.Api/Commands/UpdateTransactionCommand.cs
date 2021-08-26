using MediatR;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Cryptofolio.Api.Commands
{
    /// <summary>
    /// Models a command to update <see cref="Infrastructure.Entities.Transaction"/>.
    /// </summary>
    [ITransactionCommandValidation(ErrorMessage = "The transaction is invalid")]
    public class UpdateTransactionCommand : CommandBase, ITransactionCommand, IRequest<CommandResult>
    {
        /// <summary>
        /// The id.
        /// </summary>
        [Required(ErrorMessage = "The transaction id is required")]
        public string Id { get; set; }

        /// <summary>
        /// The transaction type.
        /// </summary>
        [Required(ErrorMessage = "The transaction type is required")]
        public string Type { get; set; }

        /// <summary>
        /// The transaction date.
        /// </summary>
        public DateTimeOffset Date { get; set; }

        /// <summary>
        /// The asset it was bought/sold/transfered.
        /// </summary>
        [StringLength(36, ErrorMessage = "The exchange id can't be greater than {0} characters")]
        [JsonPropertyName("exchange_id")]
        public string ExchangeId { get; set; }

        /// <summary>
        /// The currency used to buy/sell the asset.
        /// </summary>
        [StringLength(10, ErrorMessage = "The currency can't be greater than {0} characters")]
        public string Currency { get; set; }

        /// <summary>
        /// The price in the currency.
        /// </summary>
        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "The price can't be lesser than {1}")]
        public decimal Price { get; set; }

        /// <summary>
        /// The asset quantity.
        /// </summary>
        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "The quantity can't be lesser than {1}")]
        public decimal Qty { get; set; }

        /// <summary>
        /// The transfer source.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The transfer destination.
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// The optional note.
        /// </summary>
        public string Note { get; set; }
    }
}
