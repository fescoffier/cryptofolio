using Cryptofolio.Infrastructure.Caching;
using FluentAssertions;
using Xunit;

namespace Cryptofolio.Infrastructure.Tests.Caching
{
    public class TickerPairTest
    {
        [Fact]
        public void StringConvertOperator_Test()
        {
            // Setup
            var st = "A/B";

            // Act
            var pair = (TickerPair)st;

            // Assert
            pair.Left.Should().Be("a");
            pair.Right.Should().Be("b");
        }

        [Fact]
        public void StringConvertOperator_EmptyString_Test()
        {
            // Setup
            var st = "";

            // Act
            var pair = (TickerPair)st;

            // Assert
            pair.Left.Should().BeNull();
            pair.Right.Should().BeNull();
        }

        [Fact]
        public void StringConvertOperator_IncompletePair_Test()
        {
            // Setup
            var st = "AB";

            // Act
            var pair = (TickerPair)st;

            // Assert
            pair.Left.Should().BeNull();
            pair.Right.Should().BeNull();
        }
    }
}
