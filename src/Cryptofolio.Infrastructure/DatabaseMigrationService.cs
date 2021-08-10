using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Infrastructure
{
    /// <summary>
    /// Provides an implementation of <see cref="IHostedService"/> that applies migrations to the specified <see cref="DbContext"/>.
    /// </summary>
    /// <typeparam name="TContext">The <see cref="DbContext"/> type.</typeparam>
    public class DatabaseMigrationService<TContext> : IHostedService where TContext : DbContext
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DatabaseMigrationService<TContext>> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="DatabaseMigrationService{TContext}"/>.
        /// </summary>
        /// <param name="scopeFactory">The factory used to created new <see cref="IServiceScope"/>.</param>
        /// <param name="logger">The logger.</param>
        public DatabaseMigrationService(
            IServiceScopeFactory scopeFactory,
            ILogger<DatabaseMigrationService<TContext>> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            _logger.LogInformation("Applying database migrations to the {0} context.", typeof(TContext).Name);
            await scope.ServiceProvider.GetRequiredService<TContext>().Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("Database migrations applied on {0}.", typeof(TContext).Name);
        }

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
