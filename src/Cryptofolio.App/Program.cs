using Cryptofolio.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Text.Json;

namespace Cryptofolio.App
{
    public class Program
    {
        public static int Main(string[] args)
        {
            // The global logger is used only when the host starts and stops.
            // Otherwise, the logger factory configured in the host is used.
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: InfrastructureConstants.Logging.Theme, outputTemplate: InfrastructureConstants.Logging.OutputTemplate)
                .CreateLogger();

            try
            {
                Log.Information("Starting web host.");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly.");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(configBuilder =>
                {
                    configBuilder.AddJsonFile("appsettings.Users.json", true);
                })
                .ConfigureLogging(new JsonSerializerOptions
                {
                    Converters =
                    {
                        new IEventJsonConverter()
                    }
                })
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}
