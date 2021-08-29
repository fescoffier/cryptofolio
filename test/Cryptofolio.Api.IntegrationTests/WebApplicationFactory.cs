using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.TestsCommon;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cryptofolio.Api.IntegrationTests
{
    public class WebApplicationFactory : WebApplicationFactory<Startup>
    {
        public string DbName { get; } = Guid.NewGuid().ToString();

        public TestData Data { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Serilog:MinimumLevel:Default", "Fatal" },
                    { "ConnectionStrings:CryptofolioContext", $"Host=localhost;Database={DbName};Username=cryptofolio;Password=Pass@word1;Port=55432;IncludeErrorDetails=true" },
                    { "Kafka:Topics:Cryptofolio.Infrastructure.IEvent", Guid.NewGuid().ToString() }
                });
            });
            builder.ConfigureServices((ctx, services) =>
            {
                services.Remove(services.Single(s => s.ServiceType == typeof(Microsoft.Extensions.Internal.ISystemClock) && s.ImplementationType == typeof(Microsoft.Extensions.Internal.SystemClock)));
                var systemClockMock = new Mock<Microsoft.Extensions.Internal.ISystemClock>();
                systemClockMock.SetupGet(m => m.UtcNow).Returns(() => DateTimeOffset.UtcNow);
                services.AddSingleton(systemClockMock.Object);
                services.AddSingleton(systemClockMock);
                services.Remove(services.Single(s => s.ServiceType == typeof(IEventDispatcher) && s.ImplementationType == typeof(KafkaEventDispatcher)));
                var dispatcherMock = new Mock<IEventDispatcher>();
                dispatcherMock.Setup(m => m.DispatchAsync(It.IsAny<IEvent>())).Returns(Task.CompletedTask);
                services.AddSingleton(dispatcherMock.Object);
                services.AddSingleton(dispatcherMock);
                services
                    .AddAuthentication(TestAuthenticationHandler.AuthenticationScheme)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.AuthenticationScheme, options => { });
                services.AddSingleton(Data);
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(DatabaseMigrationService<CryptofolioContext>)));
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
            context.Currencies.RemoveRange(context.Currencies);
            context.Exchanges.RemoveRange(context.Exchanges);
            context.SaveChanges();
        }
    }
}
