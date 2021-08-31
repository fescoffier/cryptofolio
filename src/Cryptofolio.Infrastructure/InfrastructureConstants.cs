using Serilog.Sinks.SystemConsole.Themes;
using System.Collections.Generic;

namespace Cryptofolio.Infrastructure
{
    /// <summary>
    /// Provides constants for the infrastructure layer.
    /// </summary>
    public class InfrastructureConstants
    {
        /// <summary>
        /// Logging constants.
        /// </summary>
        public static class Logging
        {
            /// <summary>
            /// The console theme.
            /// </summary>
            public static readonly AnsiConsoleTheme Theme = AnsiConsoleTheme.Literate;

            /// <summary>
            /// The console output template.
            /// </summary>
            public const string OutputTemplate = "[{Timestamp:HH:mm:ss.fff} {Level:u3} {SourceContext}] {Message:lj}{NewLine}{Exception}";
        }

        /// <summary>
        /// Authentication constants.
        /// </summary>
        public static class Authentication
        {
            /// <summary>
            /// The application name.
            /// </summary>
            public const string ApplicationName = "Cryptofolio";

            /// <summary>
            /// The cookie name.
            /// </summary>
            public const string CookieName = "Cryptofolio";

            /// <summary>
            /// The Redis key.
            /// </summary>
            public const string RedisKey = "DataProtection";
        }

        /// <summary>
        /// Transactions constants.
        /// </summary>
        public static class Transactions
        {
            /// <summary>
            /// Transactions types constants.
            /// </summary>
            public static class Types
            {
                /// <summary>
                /// The buy type.
                /// </summary>
                public const string Buy = "buy";

                /// <summary>
                /// The sell type.
                /// </summary>
                public const string Sell = "sell";

                /// <summary>
                /// The transfer type.
                /// </summary>
                public const string Transfer = "transfer";
            }

            /// <summary>
            /// Transfer sources.
            /// </summary>
            public static class Sources
            {
                /// <summary>
                /// The transfer comes from the user's exchange.
                /// </summary>
                public const string MyExchange = "My exchange";

                /// <summary>
                /// The transfer comes from the user's wallet.
                /// </summary>
                public const string MyWallet = "My wallet";

                /// <summary>
                /// The transfer comes from an external source.
                /// </summary>
                public const string ExternalSource = "External source";

                /// <summary>
                /// The transfer sources list.
                /// </summary>
                public static IEnumerable<string> All { get; } = new[]
                {
                    MyExchange,
                    MyWallet,
                    ExternalSource
                };
            }

            /// <summary>
            /// Transfer destinations.
            /// </summary>
            public static class Destinations
            {
                /// <summary>
                /// The transfer goes to the user's exchange.
                /// </summary>
                public const string MyExchange = "My exchange";

                /// <summary>
                /// The transfer goes to the user's wallet.
                /// </summary>
                public const string MyWallet = "My wallet";

                /// <summary>
                /// The transfer goes to an external source.
                /// </summary>
                public const string ExternalSource = "External source";

                /// <summary>
                /// The transfer destinations list.
                /// </summary>
                public static IEnumerable<string> All { get; } = new[]
                {
                    MyExchange,
                    MyWallet,
                    ExternalSource
                };
            }
        }

        /// <summary>
        /// Events constants.
        /// </summary>
        public static class Events
        {
            /// <summary>
            /// Categories constants.
            /// </summary>
            public static class Categories
            {
                /// <summary>
                /// The asset category.
                /// </summary>
                public const string Asset = "Asset";

                /// <summary>
                /// The currency category.
                /// </summary>
                public const string Currency = "Currency";

                /// <summary>
                /// The exchange category.
                /// </summary>
                public const string Exchange = "Exchange";

                /// <summary>
                /// The transaction category.
                /// </summary>
                public const string Transaction = "Transaction";

                /// <summary>
                /// The wallet category.
                /// </summary>
                public const string Wallet = "Wallet";
            }
        }
    }
}
