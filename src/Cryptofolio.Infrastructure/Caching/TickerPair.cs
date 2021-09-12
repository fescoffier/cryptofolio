using System;

namespace Cryptofolio.Infrastructure.Caching
{
    /// <summary>
    /// Models a ticker pair.
    /// </summary>
    public struct TickerPair
    {
        /// <summary>
        /// The left part.
        /// </summary>
        public string Left { get; }

        /// <summary>
        /// The right part.
        /// </summary>
        public string Right { get; }

        /// <summary>
        /// Creates a new instance of <see cref="TickerPair"/>.
        /// </summary>
        /// <param name="left">The left part.</param>
        /// <param name="right">The right part.</param>
        public TickerPair(string left, string right)
        {
            Left = left?.ToLowerInvariant();
            Right = right?.ToLowerInvariant();
        }

        /// <summary>
        /// Convert the string into a <see cref="TickerPair"/>.
        /// </summary>
        /// <param name="pair">The pair string.</param>
        public static implicit operator TickerPair(string pair)
        {
            if (!string.IsNullOrWhiteSpace(pair))
            {
                var fragments = pair.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (fragments.Length == 2)
                {
                    return new(fragments[0], fragments[1]);
                }
            }
            return new();
        }

        /// <inheritdoc/>
        public override string ToString() => $"{Left}/{Right}";

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Left, Right);

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            obj is TickerPair pair && pair.Left == Left && pair.Right == Right;
    }
}
