using Confluent.Kafka;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Balances;
using Cryptofolio.Infrastructure.TestsCommon;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Internal;
using Moq;
using StackExchange.Redis;
using StackExchange.Redis.KeyspaceIsolation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cryptofolio.Handlers.Job.IntegrationTests
{
    public class WebApplicationFactory : WebApplicationFactory<Startup>
    {
        public string DbName { get; } = Guid.NewGuid().ToString();

        public string RedisKeyPrefix { get; set; } = Guid.NewGuid().ToString();

        public TestData Data { get; set; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Serilog:MinimumLevel:Default", "Fatal" },
                    { "ConnectionStrings:CryptofolioContext", $"Host=localhost;Database={DbName};Username=cryptofolio;Password=Pass@word1;Port=55432;IncludeErrorDetails=true" },
                    { "Kafka:Topics:Cryptofolio.Infrastructure.IEvent", Guid.NewGuid().ToString() },
                    { "Elasticsearch:Indices:Cryptofolio.Infrastructure.IEvent", Guid.NewGuid().ToString() }
                });
            });
            builder.ConfigureServices((ctx, services) =>
            {
                services.Remove(services.Single(s => s.ServiceType == typeof(ISystemClock) && s.ImplementationType == typeof(SystemClock)));
                var systemClockMock = new Mock<ISystemClock>();
                systemClockMock.SetupGet(m => m.UtcNow).Returns(() => DateTimeOffset.UtcNow);
                services.AddSingleton(systemClockMock.Object);
                services.AddSingleton(systemClockMock);
                services.AddScoped<EventTraceWriter<FakeEvent>>();
                services.AddConsumer<ComputeWalletBalanceRequest>(options =>
                {
                    options.Topic = ctx.Configuration.GetSection($"Kafka:Topics:{typeof(ComputeWalletBalanceRequest).FullName}").Get<string>();
                    options.Config = ctx.Configuration.GetSection("Kafka:Consumer").Get<ConsumerConfig>();
                });
                services.AddConsumer<BulkComputeWalletBalanceRequest>(options =>
                {
                    options.Topic = ctx.Configuration.GetSection($"Kafka:Topics:{typeof(BulkComputeWalletBalanceRequest).FullName}").Get<string>();
                    options.Config = ctx.Configuration.GetSection("Kafka:Consumer").Get<ConsumerConfig>();
                });
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(KafkaMessageHandler<IEvent>)));
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(KafkaMessageHandler<ComputeWalletBalanceRequest>)));
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(KafkaMessageHandler<BulkComputeWalletBalanceRequest>)));
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(DatabaseMigrationService<CryptofolioContext>)));
                services.AddTransient(p => p.GetRequiredService<ConnectionMultiplexer>().GetDatabase().WithKeyPrefix(RedisKeyPrefix));
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);
            using var scope = host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            context.Database.Migrate();
            return host;
        }

        public void PurgeData()
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            context.Transactions.RemoveRange(context.Transactions);
            context.Holdings.RemoveRange(context.Holdings);
            context.Wallets.RemoveRange(context.Wallets);
            context.AssetTickers.RemoveRange(context.AssetTickers);
            context.Assets.RemoveRange(context.Assets);
            context.CurrencyTickers.RemoveRange(context.CurrencyTickers);
            context.Currencies.RemoveRange(context.Currencies);
            context.Exchanges.RemoveRange(context.Exchanges);
            context.SaveChanges();
        }
    }
}
