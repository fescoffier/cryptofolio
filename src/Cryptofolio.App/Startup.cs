using Confluent.Kafka;
using Cryptofolio.Infrastructure;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System.Linq;

namespace Cryptofolio.App
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
            // Mvc.
            services.AddControllersWithViews();

            // Identity
            services
                .AddIdentityCore<IdentityUser>(options => Configuration.GetSection("Identity").Bind(options))
                .AddSignInManager()
                .AddEntityFrameworkStores<IdentityContext>();

            // Authentication
            services
                .AddAuthentication(IdentityConstants.ApplicationScheme)
                .AddCookie(IdentityConstants.ApplicationScheme, options =>
                {
                    options.LoginPath = "/auth/login";
                    options.LogoutPath = "/auth/logout";
                    options.ReturnUrlParameter = "returnUrl";

                    options.Cookie.Name = InfrastructureConstants.Authentication.CookieName;
                });
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            });
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

            // EF Core
            services.AddDbContext<IdentityContext>(options => options.UseNpgsql(Configuration.GetConnectionString("IdentityContext")));

            // Elasticsearch
            services.AddSingleton<IConnectionPool>(
                new StaticConnectionPool(
                    Configuration.GetSection("Elasticsearch:Nodes")
                        .Get<string[]>()
                        .Select(n => new Node(new(n)))
                        .ToList()
                )
            );

            // Redis
            services.AddSingleton(ConnectionMultiplexer.Connect(Configuration.GetConnectionString("Redis")));
            services.AddSingleton<IConnectionMultiplexer>(p => p.GetRequiredService<ConnectionMultiplexer>());
            services.AddTransient(p => p.GetRequiredService<ConnectionMultiplexer>().GetDatabase());

            // Healthchecks
            services
                .AddHealthChecks()
                .AddKafka(
                    Configuration.GetSection("Kafka:Producer").Get<ProducerConfig>(),
                    Configuration.GetSection("Kafka:Topics:HealthChecks").Get<string>(),
                    name: "kafka"
                )
                .AddRedis(Configuration.GetConnectionString("Redis"), name: "redis")
                .AddCheck<ElasticsearchHealthCheck>("elasticsearch");

            // Angular
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHealthChecks("/health");

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });

            using (var scope = app.ApplicationServices.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<IdentityContext>().Database.Migrate();

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var user = userManager.FindByEmailAsync("admin@cryptofolio.io").ConfigureAwait(false).GetAwaiter().GetResult();
                if (user == null)
                {
                    user = new IdentityUser
                    {
                        UserName = "admin@cryptofolio.io",
                        Email = "admin@cryptofolio.io",
                        EmailConfirmed = true
                    };
                    userManager.CreateAsync(user, "Pass@word1").ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
        }
    }
}
