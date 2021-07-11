using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Cryptofolio.Infrastructure.IntegrationTests
{
    public class TestBase : IDisposable
    {
        public IConfiguration Configuration { get; }

        public IServiceProvider Provider { get; }

        public IServiceScope Scope { get; }

        public TestBase()
        {
            Configuration = Config.BuildConfiguration(ConfigureTestConfiguration);
            Provider = BuildServiceProvider();
            Scope = Provider.CreateScope();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Scope.Dispose();
                if (Provider is IAsyncDisposable asyncDisposable)
                {
#pragma warning disable CA2012 // Use ValueTasks correctly
                    asyncDisposable.DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
#pragma warning restore CA2012 // Use ValueTasks correctly
                }
                if (Provider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        private IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection()
                .AddScoped(_ => CreateContext())
                .AddMediatR(typeof(TestBase))
                .AddTransient(typeof(ILogger<>), typeof(NullLogger<>));
            ConfigureTestServices(services);
            return services.BuildServiceProvider();
        }

        protected virtual void ConfigureTestConfiguration(IConfigurationBuilder configBuilder)
        {
        }

        protected virtual void ConfigureTestServices(IServiceCollection services)
        {
        }

        protected TService ResolveServiceFromScope<TService>() => Scope.ServiceProvider.GetRequiredService<TService>();

        protected CryptofolioContext CreateContext()
        {
            var contextOptions = new DbContextOptionsBuilder<CryptofolioContext>().UseNpgsql(Configuration.GetConnectionString("CryptofolioContext")).Options;
            var context = new CryptofolioContext(contextOptions);
            context.Database.Migrate();
            return context;
        }
    }
}
