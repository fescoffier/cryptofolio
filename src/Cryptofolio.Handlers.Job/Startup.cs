using Confluent.Kafka;
using Cryptofolio.Infrastructure;
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
                    settings.EnableDebugMode();
                }
                return settings;
            });
            services.AddSingleton<IElasticClient>(p => new ElasticClient(p.GetRequiredService<IConnectionSettingsValues>()));

            // MediatR
            services.AddMediatR(typeof(IMediator));

            // Events
            services.AddDefaultEventHandler<AssetInfosUpsertedEvent>();
            services.AddDefaultEventHandler<AssetTickerUpsertedEvent>();
            services.AddDefaultEventHandler<ExchangeInfosUpsertedEvent>();

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
