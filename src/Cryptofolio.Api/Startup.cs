using Confluent.Kafka;
using Cryptofolio.Api.Commands;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Entities;
using Elasticsearch.Net;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Internal;
using Nest;
using StackExchange.Redis;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Cryptofolio.Api
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
            // Mvc
            services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.Converters.Add(new TransactionJsonConverter());
                });

            // Authentification
            services
                .AddAuthentication(IdentityConstants.ApplicationScheme)
                .AddCookie(IdentityConstants.ApplicationScheme, options =>
                {
                    options.Cookie.Name = InfrastructureConstants.Authentication.CookieName;
                    // This disables automatic authentication challenge.
                    options.Events.OnRedirectToLogin = ctx =>
                    {
                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    };
                });
            services.AddAuthorization();
            var dataProtectionBuilder = services.AddDataProtection();
            dataProtectionBuilder.PersistKeysToStackExchangeRedis(
                ConnectionMultiplexer.Connect(Configuration.GetConnectionString("Redis")),
                InfrastructureConstants.Authentication.RedisKey
            );
            dataProtectionBuilder.SetApplicationName(InfrastructureConstants.Authentication.ApplicationName);
            if (!Environment.IsDevelopment())
            {
                // TODO: Configure key protection.
            }

            // Cors
            if (Environment.IsDevelopment())
            {
                services.AddCors(options =>
                {
                    options.AddDefaultPolicy(p => p
                        .WithOrigins(Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>())
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                    );
                });
            }

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

            // Traceability
            services.AddHttpContextAccessor();
            services.AddTransient<ISystemClock, SystemClock>();
            services.AddScoped(p => RequestContext.FromHttpContext(p.GetRequiredService<IHttpContextAccessor>().HttpContext));
            services.AddScoped<RequestContextActionFilter>();

            // MediatR
            services.AddMediatR(typeof(IMediator));
            // Wallet
            services
                .AddScoped<CreateWalletCommandHandler>()
                .AddScoped<IRequestHandler<CreateWalletCommand, CommandResult<Wallet>>>(p => p.GetRequiredService<CreateWalletCommandHandler>());
            services
                .AddScoped<UpdateWalletCommandHandler>()
                .AddScoped<IRequestHandler<UpdateWalletCommand, CommandResult>>(p => p.GetRequiredService<UpdateWalletCommandHandler>());
            services
                .AddScoped<SetSelectedWalletCommandHandler>()
                .AddScoped<IRequestHandler<SetSelectedWalletCommand, CommandResult>>(p => p.GetRequiredService<SetSelectedWalletCommandHandler>());
            services
                .AddScoped<DeleteWalletCommandHandler>()
                .AddScoped<IRequestHandler<DeleteWalletCommand, CommandResult>>(p => p.GetRequiredService<DeleteWalletCommandHandler>());
            // Transaction
            services
                .AddScoped<CreateTransactionCommandHandler>()
                .AddScoped<IRequestHandler<CreateTransactionCommand, CommandResult<Transaction>>>(p => p.GetRequiredService<CreateTransactionCommandHandler>());
            services
                .AddScoped<UpdateTransactionCommandHandler>()
                .AddScoped<IRequestHandler<UpdateTransactionCommand, CommandResult>>(p => p.GetRequiredService<UpdateTransactionCommandHandler>());
            services
                .AddScoped<DeleteTransactionCommandHandler>()
                .AddScoped<IRequestHandler<DeleteTransactionCommand, CommandResult>>(p => p.GetRequiredService<DeleteTransactionCommandHandler>());
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            foreach (var path in Configuration.GetSection("Paths").Get<string[]>())
            {
                app.UsePathBase(path);
            }

            app.UseHealthChecks("/health");

            app.UseRouting();

            if (Environment.IsDevelopment())
            {
                app.UseCors();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());
        }
    }
}
