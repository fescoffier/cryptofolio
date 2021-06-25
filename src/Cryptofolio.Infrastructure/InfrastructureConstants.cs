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
    }
}
