using Cryptofolio.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.App
{
    /// <summary>
    /// Provides an implementation of <see cref="IHostedService"/> that initializes default <see cref="IdentityUser"/>.
    /// </summary>
    public class IdentityUserService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptions<IdentityUserServiceOptions> _options;
        private readonly ILogger<IdentityUserService> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="IdentityUserService"/>.
        /// </summary>
        /// <param name="scopeFactory">The factory used to created new <see cref="IServiceScope"/>.</param>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger.</param>
        public IdentityUserService(
            IServiceScopeFactory scopeFactory,
            IOptions<IdentityUserServiceOptions> options,
            ILogger<IdentityUserService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            _logger.LogInformation("Initializing identity users.");
            foreach (var user in _options.Value.Users)
            {
                _logger.LogDebug("Checking user {0}.", user.Email);
                if (await userManager.FindByEmailAsync(user.Email) == null)
                {
                    _logger.LogDebug("User {0} doesn't exist.", user.Email);
                    _logger.LogDebug("Creating user {0}.", user.Email);
                    var result = await userManager.CreateAsync(user, user.PasswordHash);
                    if (result.Succeeded)
                    {
                        _logger.LogDebug("User {0} created.", user.Email);
                    }
                    else
                    {
                        _logger.LogError("Failed to create user {0}.", user.Email);
                        _logger.LogTraceObject("Erros", result.Errors);
                    }
                }
                else
                {
                    _logger.LogDebug("User {0} exists.", user.Email);
                }
            }
            _logger.LogInformation("Identity users initialization done.");
        }

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
