using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cryptofolio.Infrastructure
{
    /// <summary>
    /// Provides extension methods <see cref="IHostBuilder"/>.
    /// </summary>
    public static class LoggingHostBuilderExtensions
    {
        /// <summary>
        /// Configures Serilog.
        /// </summary>
        /// <param name="builder">The host builder.</param>
        /// <param name="traceSerializationOptions">The options to serialize traces.</param>
        /// <returns>The host builder.</returns>
        public static IHostBuilder ConfigureLogging(this IHostBuilder builder, JsonSerializerOptions traceSerializationOptions = default)
        {
            LoggingExtensions.SerializerOptions = traceSerializationOptions ?? new();
            LoggingExtensions.SerializerOptions.WriteIndented = true;
            LoggingExtensions.SerializerOptions.IgnoreNullValues = true;
            LoggingExtensions.SerializerOptions.Converters.Add(new JsonStringEnumConverter());

            return builder
                .UseSerilog((context, loggerConfig) =>
                {
                    loggerConfig.ReadFrom.Configuration(context.Configuration);
                    loggerConfig.Enrich.FromLogContext();
                    loggerConfig.WriteTo.Console(theme: InfrastructureConstants.Logging.Theme, outputTemplate: InfrastructureConstants.Logging.OutputTemplate);

                    if (!context.HostingEnvironment.IsDevelopment())
                    {
                        var assembly = Assembly.GetEntryAssembly();
                        loggerConfig.Enrich.WithProperty("Assembly", assembly.GetName().Name);
                        loggerConfig.Enrich.WithProperty("Version", $"{assembly.GetName().Version.Major}.{assembly.GetName().Version.Minor}.{assembly.GetName().Version.Build}");
                        loggerConfig.WriteTo.Elasticsearch(new(new Uri(context.Configuration.GetValue<string>("Serilog:Elasticsearch:Url")))
                        {
                            AutoRegisterTemplate = true,
                            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                            IndexFormat = context.Configuration.GetValue<string>("Serilog:Elasticsearch:IndexFormat")
                        });
                    }
                });
        }
    }
}
