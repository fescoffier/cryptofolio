using Confluent.Kafka;
using Cryptofolio.Infrastructure;
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
                    options.LoginPath = "/login";
                    options.LogoutPath = "/logout";
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
            services.AddDbContext<IdentityContext>(builder =>
            {
                builder.UseNpgsql(Configuration.GetConnectionString("IdentityContext"));
                if (Environment.IsDevelopment())
                {
                    builder.EnableSensitiveDataLogging();
                    builder.EnableDetailedErrors();
                }
            });
            services.AddHostedService<DatabaseMigrationService<IdentityContext>>();

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
                .AddRedis(Configuration.GetConnectionString("Redis"), name: "redis");

            // Angular
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services.Configure<ApiOptions>(Configuration.GetSection("Api"));
            services.Configure<IdentityUserServiceOptions>(Configuration.GetSection("Identity"));
            services.PostConfigure<IdentityUserServiceOptions>(options => options.Users ??= Enumerable.Empty<IdentityUser>());
            services.AddHostedService<IdentityUserService>();
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
        }
    }
}
