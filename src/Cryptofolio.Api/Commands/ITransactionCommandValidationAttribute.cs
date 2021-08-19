using Cryptofolio.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cryptofolio.Api.Commands
{
    /// <summary>
    /// Provides validation for <see cref="ITransactionCommand"/> implementations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ITransactionCommandValidationAttribute : ValidationAttribute
    {
        /// <inheritdoc/>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var errorMemberNames = new HashSet<string>();

            if (value is ITransactionCommand command)
            {
                if (command.Type == CommandConstants.Transaction.Types.Buy)
                {
                    ValidateBuy(command, errorMemberNames);
                }
                else if (command.Type == CommandConstants.Transaction.Types.Sell)
                {
                    ValidateSell(command, errorMemberNames);
                }
                else if (command.Type == CommandConstants.Transaction.Types.Transfer)
                {
                    ValidateTransfer(command, errorMemberNames);
                }
                else
                {
                    errorMemberNames.Add(nameof(ITransactionCommand.Type));
                }
            }

            return errorMemberNames.Count > 0 ? new ValidationResult(ErrorMessage, errorMemberNames) : ValidationResult.Success;
        }

        private void ValidateBuy(ITransactionCommand command, HashSet<string> errorMemberNames)
        {
            if (string.IsNullOrWhiteSpace(command.Currency))
            {
                errorMemberNames.Add(nameof(ITransactionCommand.Currency));
            }
            if (command.Price == default)
            {
                errorMemberNames.Add(nameof(ITransactionCommand.Price));
            }
            if (command.Qty == default)
            {
                errorMemberNames.Add(nameof(ITransactionCommand.Qty));
            }
        }

        private void ValidateSell(ITransactionCommand command, HashSet<string> errorMemberNames)
        {
            if (string.IsNullOrWhiteSpace(command.Currency))
            {
                errorMemberNames.Add(nameof(ITransactionCommand.Currency));
            }
            if (command.Price == default)
            {
                errorMemberNames.Add(nameof(ITransactionCommand.Price));
            }
            if (command.Qty == default)
            {
                errorMemberNames.Add(nameof(ITransactionCommand.Qty));
            }
        }

        private void ValidateTransfer(ITransactionCommand command, HashSet<string> errorMemberNames)
        {
            if (string.IsNullOrWhiteSpace(command.Source))
            {
                errorMemberNames.Add(nameof(ITransactionCommand.Source));
            }
            else if (command.Source == InfrastructureConstants.Transactions.Sources.MyExchange && string.IsNullOrWhiteSpace(command.ExchangeId))
            {
                errorMemberNames.Add(nameof(ITransactionCommand.ExchangeId));
            }

            if (string.IsNullOrWhiteSpace(command.Destination))
            {
                errorMemberNames.Add(nameof(ITransactionCommand.Destination));
            }
            else if (command.Destination == InfrastructureConstants.Transactions.Sources.MyExchange && string.IsNullOrWhiteSpace(command.ExchangeId))
            {
                errorMemberNames.Add(nameof(ITransactionCommand.ExchangeId));
            }

            if (command.Source == command.Destination)
            {
                errorMemberNames.Add(nameof(ITransactionCommand.Source));
                errorMemberNames.Add(nameof(ITransactionCommand.Destination));
            }
        }
    }
}
