using Serilog.Sinks.SystemConsole.Themes;

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
                /// The exchange category.
                /// </summary>
                public const string Exchange = "Exchange";

                /// <summary>
                /// The wallet category.
                /// </summary>
                public const string Wallet = "Wallet";
            }
        }
    }
}
