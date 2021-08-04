using Cryptofolio.Api.Commands;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace Cryptofolio.Api.Tests.Commands
{
    public class CommandResultTests
    {
        [Fact]
        public void Success_Test()
        {
            // Act
            var result = CommandResult.Success();

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Failed_Enumerable_Test()
        {
            // Setup
            var error = "Error";

            // Act
            var result = CommandResult.Failed(new[] { error }.AsEnumerable());

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().Contain(error);
        }

        [Fact]
        public void Failed_ArrayParams_Test()
        {
            // Setup
            var error = "Error";

            // Act
            var result = CommandResult.Failed(error);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().Contain(error);
        }

        [Fact]
        public void Success_Generic_Test()
        {
            // Setup
            var value = "Value";

            // Act
            var result = CommandResult<string>.Success(value);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().Be(value);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Failed_Generic_Enumerable_Test()
        {
            // Setup
            var error = "Error";

            // Act
            var result = CommandResult<string>.Failed(new[] { error }.AsEnumerable());

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Errors.Should().Contain(error);
        }

        [Fact]
        public void Failed_Generic_ArrayParams_Test()
        {
            // Setup
            var error = "Error";

            // Act
            var result = CommandResult<string>.Failed(error);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Errors.Should().Contain(error);
        }
    }
}
