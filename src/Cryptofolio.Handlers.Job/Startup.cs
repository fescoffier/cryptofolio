using Confluent.Kafka;
using Cryptofolio.Handlers.Job.Currencies;
using Cryptofolio.Handlers.Job.Transactions;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Caching;
using Cryptofolio.Infrastructure.Entities;
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
using Nest;
using StackExchange.Redis;
using System.Linq;
using System.Text.Json;

namespace Cryptofolio.Handlers.Job
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
            services.AddConsumer<IEvent>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(IEvent).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Consumer").Get<ConsumerConfig>();
                options.ValueSerilializerOptions = new()
                {
                    Converters =
                    {
                        new TransactionPolymorphicJsonConverter()
                    }
                };
            });

            // Elasticsearch
            services.Configure<ElasticsearchOptions>(Configuration.GetSection("Elasticsearch"));
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
                var indexTemplate = Configuration.GetSection($"Elasticsearch:Indices:{typeof(IEvent).FullName}").Get<string>();
                var serializationOptions = Configuration.GetSection("Elasticsearch:SerializationOptions").Get<JsonSerializerOptions>() ?? new();
                serializationOptions.Converters.Add(new IEventJsonConverter());
                var settings = new ConnectionSettings(
                    p.GetRequiredService<IConnectionPool>(),
                    (builtIn, settings) => new ElasticsearchSerializer(serializationOptions)
                );
                if (Environment.IsDevelopment())
                {
                    settings.DefaultMappingFor<IEvent>(config => config.IndexName(indexTemplate));
                    settings.EnableDebugMode();
                }
                return settings;
            });
            services.AddSingleton<IElasticClient>(p => new ElasticClient(p.GetRequiredService<IConnectionSettingsValues>()));

            // Redis
            services.AddSingleton(ConnectionMultiplexer.Connect(Configuration.GetConnectionString("Redis")));
            services.AddSingleton<IConnectionMultiplexer>(p => p.GetRequiredService<ConnectionMultiplexer>());
            services.AddTransient(p => p.GetRequiredService<ConnectionMultiplexer>().GetDatabase());

            // MediatR
            services.AddMediatR(typeof(IMediator));

            // Caching
            services.AddTransient<AssetTickerCache>();
            services.AddTransient<CurrencyTickerCache>();

            // Events
            services.AddDefaultEventHandler<AssetInfosUpsertedEvent>();
            services.AddDefaultEventHandler<AssetTickersUpsertedEvent>();
            services.AddEventHandler<CurrencyTickersUpsertedEvent, CurrencyTickersUpsertedEventHandler>();
            services.AddDefaultEventHandler<ExchangeInfosUpsertedEvent>();
            services.AddEventHandler<TransactionCreatedEvent, TransactionEventHandler>();
            services.AddEventHandler<TransactionUpdatedEvent, TransactionEventHandler>();
            services.AddEventHandler<TransactionDeletedEvent, TransactionEventHandler>();

            // Healthchecks
            services
                .AddHealthChecks()
                .AddNpgSql(Configuration.GetConnectionString("CryptofolioContext"), name: "db")
                .AddKafka(
                    Configuration.GetSection("Kafka:Producer").Get<ProducerConfig>(),
                    Configuration.GetSection("Kafka:Topics:HealthChecks").Get<string>(),
                    name: "kafka"
                )
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
