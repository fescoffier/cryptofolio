using Confluent.Kafka;
using Cryptofolio.Collector.Job.Data;
using Cryptofolio.Collector.Job.IntegrationTests.Data;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Internal;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cryptofolio.Collector.Job.IntegrationTests
{
    public class WebApplicationFactory : WebApplicationFactory<Startup>
    {
        public string DbName { get; } = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "ConnectionStrings:CryptofolioContext", $"Host=localhost;Database={DbName};Username=cryptofolio;Password=Pass@word1;Port=55432;IncludeErrorDetails=true" },
                    { "Kafka:Topics:Cryptofolio.Infrastructure.IEvent", Guid.NewGuid().ToString() },
                    { "Kafka:Topics:Cryptofolio.Infrastructure.Data.AssetDataRequest", Guid.NewGuid().ToString() },
                    { "Kafka:Topics:Cryptofolio.Infrastructure.Data.AssetTickerDataRequest", Guid.NewGuid().ToString() },
                    { "Kafka:Topics:Cryptofolio.Infrastructure.Data.ExchangeDataRequest", Guid.NewGuid().ToString() },
                    { "Kafka:Topics:Cryptofolio.Collector.Job.IntegrationTests.Data.TestDataRequest", Guid.NewGuid().ToString() },
                    { "Data:Schedules:Cryptofolio.Infrastructure.Data.AssetDataRequest", "* * * * *" },
                    { "Data:Schedules:Cryptofolio.Infrastructure.Data.AssetTickerDataRequest", "* * * * *" },
                    { "Data:Schedules:Cryptofolio.Infrastructure.Data.ExchangeDataRequest", "* * * * *" },
                    { "Data:Schedules:Cryptofolio.Collector.Job.IntegrationTests.Data.TestDataRequest", "* * * * *" }
                });
            });
            builder.ConfigureServices((ctx, services) =>
            {
                services.Remove(services.Single(s => s.ServiceType == typeof(ISystemClock) && s.ImplementationType == typeof(SystemClock)));
                var systemClockMock = new Mock<ISystemClock>();
                systemClockMock.SetupGet(m => m.UtcNow).Returns(() => DateTimeOffset.UtcNow);
                services.AddSingleton(systemClockMock.Object);
                services.AddSingleton(systemClockMock);
                services.AddProducer<TestDataRequest>(options =>
                {
                    options.Topic = ctx.Configuration.GetSection($"Kafka:Topics:{typeof(TestDataRequest).FullName}").Get<string>();
                    options.Config = ctx.Configuration.GetSection("Kafka:Producer").Get<ProducerConfig>();
                });
                services.AddConsumer<TestDataRequest>(options =>
                {
                    options.Topic = ctx.Configuration.GetSection($"Kafka:Topics:{typeof(TestDataRequest).FullName}").Get<string>();
                    options.Config = ctx.Configuration.GetSection("Kafka:Consumer").Get<ConsumerConfig>();
                });
                services.AddSingleton<AssetDataRequestScheduler>();
                services.AddSingleton<AssetTickerDataRequestScheduler>();
                services.AddSingleton<ExchangeDataRequestScheduler>();
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(KafkaMessageHandler<AssetDataRequest>)));
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(AssetDataRequestScheduler)));
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(KafkaMessageHandler<AssetTickerDataRequest>)));
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(AssetTickerDataRequestScheduler)));
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(KafkaMessageHandler<ExchangeDataRequest>)));
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(ExchangeDataRequestScheduler)));
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(KafkaMessageHandler<TestDataRequest>)));
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(DatabaseMigrationService<CryptofolioContext>)));
                services.AddSingleton<TestDataRequestScheduler>();
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
    }
}
