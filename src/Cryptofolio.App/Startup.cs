using Confluent.Kafka;
using Cryptofolio.Infrastructure;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Builder;
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
                .AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<IdentityContext>();

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

            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

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
