using CoinGecko.Clients;
using CoinGecko.Interfaces;
using Confluent.Kafka;
using Cryptofolio.Collector.Job.Coingecko;
using Cryptofolio.Collector.Job.Data;
using Cryptofolio.Collector.Job.Fixer;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Data;
using Elasticsearch.Net;
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
using Nest;
using StackExchange.Redis;
using System.Linq;
using System.Text.Json;

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
            services.AddHostedService<DatabaseMigrationService<CryptofolioContext>>();

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
            services.AddProducer<CurrencyTickerDataRequest>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(CurrencyTickerDataRequest).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Producer").Get<ProducerConfig>();
            });
            services.AddConsumer<CurrencyTickerDataRequest>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(CurrencyTickerDataRequest).FullName}").Get<string>();
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

            // Elasticsearch
            services.AddSingleton<IConnectionPool>(
                new StaticConnectionPool(
                    Configuration.GetSection("Elasticsearch:Nodes")
                        .Get<string[]>()
                        .Select(n => new Node(new(n)))
                        .ToList()
                )
            );
            services.AddSingleton<IConnectionSettingsValues>(p =>
            {
                var settings = new ConnectionSettings(p.GetRequiredService<IConnectionPool>());
                if (Environment.IsDevelopment())
                {
                    settings.EnableDebugMode();
                }
                return settings;
            });
            services.AddSingleton<IElasticClient>(p => new ElasticClient(p.GetRequiredService<IConnectionSettingsValues>()));

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

            // Fixer
            services.Configure<FixerOptions>(Configuration.GetSection("Fixer"));
            services.PostConfigure<FixerOptions>(options =>
            {
                options.SerializerOptions ??= new();
                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.SerializerOptions.Converters.Add(new FixerDateTimeOffsetJsonConverter());
            });
            services.AddHttpClient<FixerClient>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptionsMonitor<FixerOptions>>().CurrentValue;
                client.BaseAddress = new(options.ApiUri);
            });

            // MediatR
            services.AddMediatR(typeof(Startup));
            // Assets
            services.AddScoped<AssetDataRequestHandler>();
            services.AddScoped<IPipelineBehavior<AssetDataRequest, Unit>>(p => p.GetRequiredService<AssetDataRequestHandler>());
            services.AddScoped<AssetTickerDataRequestHandler>();
            services.AddScoped<IPipelineBehavior<AssetTickerDataRequest, Unit>>(p => p.GetRequiredService<AssetTickerDataRequestHandler>());
            // Currencies
            services.AddScoped<CurrencyTickerDataRequestHandler>();
            services.AddScoped<IPipelineBehavior<CurrencyTickerDataRequest, Unit>>(p => p.GetRequiredService<CurrencyTickerDataRequestHandler>());
            // Exchanges
            services.AddScoped<ExchangeDataRequestHandler>();
            services.AddScoped<IPipelineBehavior<ExchangeDataRequest, Unit>>(p => p.GetRequiredService<ExchangeDataRequestHandler>());

            // Data requests scheduler.
            services.Configure<DataRequestSchedulerOptions>(Configuration.GetSection("Data"));
            services.AddHostedService<AssetDataRequestScheduler>();
            services.AddHostedService<AssetTickerDataRequestScheduler>();
            services.AddHostedService<CurrencyTickerDataRequestScheduler>();
            services.AddHostedService<ExchangeDataRequestScheduler>();

            // Healthchecks
            services
                .AddHealthChecks()
                .AddNpgSql(Configuration.GetConnectionString("CryptofolioContext"), name: "db")
                .AddKafka(
                    Configuration.GetSection("Kafka:Producer").Get<ProducerConfig>(),
                    Configuration.GetSection("Kafka:Topics:HealthChecks").Get<string>(),
                    name: "kafka"
                )
                .AddRedis(Configuration.GetConnectionString("Redis"), name: "redis")
                .AddCheck<ElasticsearchHealthCheck>("elasticsearch");

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
