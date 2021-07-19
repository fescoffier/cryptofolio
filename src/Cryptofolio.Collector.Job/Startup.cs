using CoinGecko.Clients;
using CoinGecko.Interfaces;
using Confluent.Kafka;
using Cryptofolio.Collector.Job.Data;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

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
                builder.UseNpgsql(Configuration.GetConnectionString("CryptofolioContext"));
                if (Environment.IsDevelopment())
                {
                    builder.EnableSensitiveDataLogging();
                    builder.EnableDetailedErrors();
                }
            });

            // Kafka
            services.AddProducer<IEvent>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(IEvent).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Producer").Get<ProducerConfig>();
            });
            services.AddProducer<AssetDataRequest>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(AssetDataRequest).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Producer").Get<ProducerConfig>();
            });
            services.AddConsumer<AssetDataRequest>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(AssetDataRequest).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Consumer").Get<ConsumerConfig>();
            });
            services.AddProducer<AssetTickerDataRequest>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(AssetTickerDataRequest).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Producer").Get<ProducerConfig>();
            });
            services.AddConsumer<AssetTickerDataRequest>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(AssetTickerDataRequest).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Consumer").Get<ConsumerConfig>();
            });
            services.AddProducer<ExchangeDataRequest>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(ExchangeDataRequest).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Producer").Get<ProducerConfig>();
            });
            services.AddConsumer<ExchangeDataRequest>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(ExchangeDataRequest).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Consumer").Get<ConsumerConfig>();
            });
            services.AddTransient<IEventDispatcher, KafkaEventDispatcher>();

            // Redis
            services.AddSingleton(ConnectionMultiplexer.Connect(Configuration.GetConnectionString("Redis")));
            services.AddSingleton<IConnectionMultiplexer>(p => p.GetRequiredService<ConnectionMultiplexer>());
            services.AddTransient(p => p.GetRequiredService<ConnectionMultiplexer>().GetDatabase());

            // Coingecko
            services.Configure<CoingeckoOptions>(Configuration.GetSection("Coingecko"));
            services.AddScoped<CoingeckoHttpClientHandler>();
            services
                .AddHttpClient<ICoinsClient, CoinsClient>((provider, client) =>
                {
                    var options = provider.GetRequiredService<IOptionsMonitor<CoingeckoOptions>>().CurrentValue;
                    client.BaseAddress = new(options.ApiUri);
                })
                .ConfigurePrimaryHttpMessageHandler<CoingeckoHttpClientHandler>();
            services
                .AddHttpClient<ISimpleClient, SimpleClient>((provider, client) =>
                {
                    var options = provider.GetRequiredService<IOptionsMonitor<CoingeckoOptions>>().CurrentValue;
                    client.BaseAddress = new(options.ApiUri);
                })
                .ConfigurePrimaryHttpMessageHandler<CoingeckoHttpClientHandler>();
            services
                .AddHttpClient<IExchangesClient, ExchangesClient>((provider, client) =>
                {
                    var options = provider.GetRequiredService<IOptionsMonitor<CoingeckoOptions>>().CurrentValue;
                    client.BaseAddress = new(options.ApiUri);
                })
                .ConfigurePrimaryHttpMessageHandler<CoingeckoHttpClientHandler>();

            // MediatR
            services.AddMediatR(typeof(Startup));
            // Assets
            services.AddScoped<AssetDataRequestHandler>();
            services.AddScoped<IPipelineBehavior<AssetDataRequest, Unit>>(p => p.GetRequiredService<AssetDataRequestHandler>());
            services.AddScoped<AssetTickerDataRequestHandler>();
            services.AddScoped<IPipelineBehavior<AssetTickerDataRequest, Unit>>(p => p.GetRequiredService<AssetTickerDataRequestHandler>());
            // Exchanges
            services.AddScoped<ExchangeDataRequestHandler>();
            services.AddScoped<IPipelineBehavior<ExchangeDataRequest, Unit>>(p => p.GetRequiredService<ExchangeDataRequestHandler>());

            // Data requests scheduler.
            services.Configure<DataRequestSchedulerOptions>(Configuration.GetSection("Data"));
            services.AddHostedService<AssetDataRequestScheduler>();
            services.AddHostedService<AssetTickerDataRequestScheduler>();
            services.AddHostedService<ExchangeDataRequestScheduler>();

            // Healthchecks
            services
                .AddHealthChecks()
                .AddNpgSql(Configuration.GetConnectionString("CryptofolioContext"), name: "db")
                .AddKafka(
                    Configuration.GetSection("Kafka:Producer").Get<ProducerConfig>(),
                    Configuration.GetSection("Kafka:Topics:HealthChecks").Get<string>()
                );
            // TODO: Elasticsearch checks.

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
