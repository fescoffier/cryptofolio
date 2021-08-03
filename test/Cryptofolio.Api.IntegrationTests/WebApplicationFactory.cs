using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.Extensions.Hosting;
using Cryptofolio.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;

namespace Cryptofolio.Api.IntegrationTests
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
