using CoinGecko.Clients;
using CoinGecko.Interfaces;
using Confluent.Kafka;
using Cryptofolio.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;

namespace Cryptofolio.Collector.Job
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // EF Core
            services.AddDbContext<CryptofolioContext>(builder =>
            {
                builder.UseNpgsql(Configuration.GetConnectionString("Cryptofolio"));
                if (Environment.IsDevelopment())
                {
                    builder.EnableSensitiveDataLogging();
                    builder.EnableDetailedErrors();
                }
            });

            // Coingecko
            services.Configure<CoingeckoOptions>(Configuration.GetSection("Coingecko"));
            services
                .AddHttpClient<ICoinsClient, CoinsClient>((provider, client) =>
                {
                    var options = provider.GetRequiredService<IOptionsMonitor<CoingeckoOptions>>().CurrentValue;
                    client.BaseAddress = new(options.ApiUri);
                });

            // Healthchecks
            services
                .AddHealthChecks()
                .AddNpgSql(Configuration.GetConnectionString("Cryptofolio"), name: "db")
                .AddKafka(
                    Configuration.GetSection("Kafka:Producer").Get<ProducerConfig>(),
                    Configuration.GetSection("Kafka:Topics:HealthChecks").Get<string>()
                );

            services.TryAddSingleton<ISystemClock, SystemClock>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHealthChecks("/health");
        }
    }
}
