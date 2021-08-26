using Cryptofolio.Api.Commands;
using Cryptofolio.Infrastructure;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Cryptofolio.Api.Tests.Commands
{
    public class ITransactionCommandValidationAttributeTests
    {
        private const string ErrorMessage = "The command is invalid";

        [Fact]
        public void IsValid_Buy_Test()
        {
            // Setup
            var command = new FakeTransactionCommand
            {
                Type = CommandConstants.Transaction.Types.Buy,
                ExchangeId = "exchange1",
                CurrencyId = Guid.NewGuid().ToString(),
                Price = 1,
                Qty = 1
            };
            var validationContext = new ValidationContext(command);
            var validationResults = new List<ValidationResult>();

            // Act
            var result = Validator.TryValidateObject(command, validationContext, validationResults);

            // Assert
            result.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void IsValid_Buy_InvalidExchangeId_Test()
        {
            // Setup
            var command = new FakeTransactionCommand
            {
                Type = CommandConstants.Transaction.Types.Buy,
                CurrencyId = Guid.NewGuid().ToString(),
                Price = 1,
                Qty = 1
            };
            var validationContext = new ValidationContext(command);
            var validationResults = new List<ValidationResult>();

            // Act
            var result = Validator.TryValidateObject(command, validationContext, validationResults);

            // Assert
            result.Should().BeFalse();
            validationResults.Should().ContainEquivalentOf(new ValidationResult(ErrorMessage, new[] { nameof(ITransactionCommand.ExchangeId) }));
        }

        [Fact]
        public void IsValid_Buy_InvalidCurrency_Test()
        {
            // Setup
            var command = new FakeTransactionCommand
            {
                Type = CommandConstants.Transaction.Types.Buy,
                ExchangeId = "exchange1",
                Price = 1,
                Qty = 1
            };
            var validationContext = new ValidationContext(command);
            var validationResults = new List<ValidationResult>();

            // Act
            var result = Validator.TryValidateObject(command, validationContext, validationResults);

            // Assert
            result.Should().BeFalse();
            validationResults.Should().ContainEquivalentOf(new ValidationResult(ErrorMessage, new[] { nameof(ITransactionCommand.CurrencyId) }));
        }

        [Fact]
        public void IsValid_Buy_InvalidPrice_Test()
        {
            // Setup
            var command = new FakeTransactionCommand
            {
                Type = CommandConstants.Transaction.Types.Buy,
                ExchangeId = "exchange1",
                CurrencyId = Guid.NewGuid().ToString(),
                Qty = 1
            };
            var validationContext = new ValidationContext(command);
            var validationResults = new List<ValidationResult>();

            // Act
            var result = Validator.TryValidateObject(command, validationContext, validationResults);

            // Assert
            result.Should().BeFalse();
            validationResults.Should().ContainEquivalentOf(new ValidationResult(ErrorMessage, new[] { nameof(ITransactionCommand.Price) }));
        }

        [Fact]
        public void IsValid_Buy_InvalidQty_Test()
        {
            // Setup
            var command = new FakeTransactionCommand
            {
                Type = CommandConstants.Transaction.Types.Buy,
                ExchangeId = "exchange1",
                CurrencyId = Guid.NewGuid().ToString(),
                Price = 1
            };
            var validationContext = new ValidationContext(command);
            var validationResults = new List<ValidationResult>();

            // Act
            var result = Validator.TryValidateObject(command, validationContext, validationResults);

            // Assert
            result.Should().BeFalse();
            validationResults.Should().ContainEquivalentOf(new ValidationResult(ErrorMessage, new[] { nameof(ITransactionCommand.Qty) }));
        }

        [Fact]
        public void IsValid_Sell_Test()
        {
            // Setup
            var command = new FakeTransactionCommand
            {
                Type = CommandConstants.Transaction.Types.Sell,
                ExchangeId = "exchange1",
                CurrencyId = Guid.NewGuid().ToString(),
                Price = 1,
                Qty = 1
            };
            var validationContext = new ValidationContext(command);
            var validationResults = new List<ValidationResult>();

            // Act
            var result = Validator.TryValidateObject(command, validationContext, validationResults);

            // Assert
            result.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void IsValid_Sell_InvalidExchangeId_Test()
        {
            // Setup
            var command = new FakeTransactionCommand
            {
                Type = CommandConstants.Transaction.Types.Sell,
                CurrencyId = Guid.NewGuid().ToString(),
                Price = 1,
                Qty = 1
            };
            var validationContext = new ValidationContext(command);
            var validationResults = new List<ValidationResult>();

            // Act
            var result = Validator.TryValidateObject(command, validationContext, validationResults);

            // Assert
            result.Should().BeFalse();
            validationResults.Should().ContainEquivalentOf(new ValidationResult(ErrorMessage, new[] { nameof(ITransactionCommand.ExchangeId) }));
        }

        [Fact]
        public void IsValid_Sell_InvalidCurrency_Test()
        {
            // Setup
            var command = new FakeTransactionCommand
            {
                Type = CommandConstants.Transaction.Types.Sell,
                ExchangeId = "exchange1",
                Price = 1,
                Qty = 1
            };
            var validationContext = new ValidationContext(command);
            var validationResults = new List<ValidationResult>();

            // Act
            var result = Validator.TryValidateObject(command, validationContext, validationResults);

            // Assert
            result.Should().BeFalse();
            validationResults.Should().ContainEquivalentOf(new ValidationResult(ErrorMessage, new[] { nameof(ITransactionCommand.CurrencyId) }));
        }

        [Fact]
        public void IsValid_Sell_InvalidPrice_Test()
        {
            // Setup
            var command = new FakeTransactionCommand
            {
                Type = CommandConstants.Transaction.Types.Sell,
                ExchangeId = "exchange1",
                CurrencyId = Guid.NewGuid().ToString(),
                Qty = 1
            };
            var validationContext = new ValidationContext(command);
            var validationResults = new List<ValidationResult>();

            // Act
            var result = Validator.TryValidateObject(command, validationContext, validationResults);

            // Assert
            result.Should().BeFalse();
            validationResults.Should().ContainEquivalentOf(new ValidationResult(ErrorMessage, new[] { nameof(ITransactionCommand.Price) }));
        }

        [Fact]
        public void IsValid_Sell_InvalidQty_Test()
        {
            // Setup
            var command = new FakeTransactionCommand
            {
                Type = CommandConstants.Transaction.Types.Sell,
                ExchangeId = "exchange1",
                CurrencyId = Guid.NewGuid().ToString(),
                Price = 1
            };
            var validationContext = new ValidationContext(command);
            var validationResults = new List<ValidationResult>();

            // Act
            var result = Validator.TryValidateObject(command, validationContext, validationResults);

            // Assert
            result.Should().BeFalse();
            validationResults.Should().ContainEquivalentOf(new ValidationResult(ErrorMessage, new[] { nameof(ITransactionCommand.Qty) }));
        }

        [Fact]
        public void IsValid_Transfer_Test()
        {
            // Setup
            var command = new FakeTransactionCommand
            {
                Type = CommandConstants.Transaction.Types.Transfer,
                Source = InfrastructureConstants.Transactions.Sources.ExternalSource,
                Destination = InfrastructureConstants.Transactions.Sources.MyWallet
            };
            var validationContext = new ValidationContext(command);
            var validationResults = new List<ValidationResult>();

            // Act
            var result = Validator.TryValidateObject(command, validationContext, validationResults);

            // Assert
            result.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void IsValid_Transfer_InvalidSource_Test()
        {
            // Setup
            var command = new FakeTransactionCommand
            {
                Type = CommandConstants.Transaction.Types.Transfer,
                Destination = InfrastructureConstants.Transactions.Sources.MyWallet
            };
            var validationContext = new ValidationContext(command);
            var validationResults = new List<ValidationResult>();

            // Act
            var result = Validator.TryValidateObject(command, validationContext, validationResults);

            // Assert
            result.Should().BeFalse();
            validationResults.Should().ContainEquivalentOf(new ValidationResult(ErrorMessage, new[] { nameof(ITransactionCommand.Source) }));
        }

        [Fact]
        public void IsValid_Transfer_InvalidSourceExchange_Test()
        {
            // Setup
            var command = new FakeTransactionCommand
            {
                Type = CommandConstants.Transaction.Types.Transfer,
                Source = InfrastructureConstants.Transactions.Sources.MyExchange,
                Destination = InfrastructureConstants.Transactions.Sources.MyWallet
            };
            var validationContext = new ValidationContext(command);
            var validationResults = new List<ValidationResult>();

            // Act
            var result = Validator.TryValidateObject(command, validationContext, validationResults);

            // Assert
            result.Should().BeFalse();
            validationResults.Should().ContainEquivalentOf(new ValidationResult(ErrorMessage, new[] { nameof(ITransactionCommand.ExchangeId) }));
        }

        [Fact]
        public void IsValid_Transfer_InvalidDestination_Test()
        {
            // Setup
            var command = new FakeTransactionCommand
            {
                Type = CommandConstants.Transaction.Types.Transfer,
                Source = InfrastructureConstants.Transactions.Sources.ExternalSource
            };
            var validationContext = new ValidationContext(command);
            var validationResults = new List<ValidationResult>();

            // Act
            var result = Validator.TryValidateObject(command, validationContext, validationResults);

            // Assert
            result.Should().BeFalse();
            validationResults.Should().ContainEquivalentOf(new ValidationResult(ErrorMessage, new[] { nameof(ITransactionCommand.Destination) }));
        }

        [Fact]
        public void IsValid_Transfer_InvalidDestinationExchange_Test()
        {
            // Setup
            var command = new FakeTransactionCommand
            {
                Type = CommandConstants.Transaction.Types.Transfer,
                Source = InfrastructureConstants.Transactions.Sources.MyWallet,
                Destination = InfrastructureConstants.Transactions.Sources.MyExchange
            };
            var validationContext = new ValidationContext(command);
            var validationResults = new List<ValidationResult>();

            // Act
            var result = Validator.TryValidateObject(command, validationContext, validationResults);

            // Assert
            result.Should().BeFalse();
            validationResults.Should().ContainEquivalentOf(new ValidationResult(ErrorMessage, new[] { nameof(ITransactionCommand.ExchangeId) }));
        }

        [Fact]
        public void IsValid_Transfer_InvalidSourceAndDestination_Test()
        {
            // Setup
            var command = new FakeTransactionCommand
            {
                Type = CommandConstants.Transaction.Types.Transfer,
                Source = InfrastructureConstants.Transactions.Sources.MyWallet,
                Destination = InfrastructureConstants.Transactions.Sources.MyWallet
            };
            var validationContext = new ValidationContext(command);
            var validationResults = new List<ValidationResult>();

            // Act
            var result = Validator.TryValidateObject(command, validationContext, validationResults);

            // Assert
            result.Should().BeFalse();
            validationResults.Should().ContainEquivalentOf(new ValidationResult(ErrorMessage, new[] { nameof(ITransactionCommand.Source), nameof(ITransactionCommand.Destination) }));
        }

        [ITransactionCommandValidation(ErrorMessage = ErrorMessage)]
        private class FakeTransactionCommand : ITransactionCommand
        {
            public string Type { get; set; }

            public DateTimeOffset Date { get; set; }

            public string WalletId { get; set; }

            public string AssetId { get; set; }

            public string ExchangeId { get; set; }

            public string CurrencyId { get; set; }

            public decimal Price { get; set; }

            public decimal Qty { get; set; }

            public string Source { get; set; }

            public string Destination { get; set; }

            public string Note { get; set; }
        }
    }
}
