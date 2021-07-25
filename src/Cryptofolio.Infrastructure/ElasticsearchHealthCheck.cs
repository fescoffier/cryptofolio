using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Infrastructure
{
    /// <summary>
    /// Provides an implementation of <see cref="IHealthCheck"/> to health check Elasticsearch.
    /// </summary>
    public class ElasticsearchHealthCheck : IHealthCheck
    {
        private Indices _indices;
        private Time _timeout;

        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly IElasticClient _elasticClient;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Creates a new instance of <see cref="ElasticsearchHealthCheck"/>.
        /// </summary>
        /// <param name="elasticClient">The elastic client.</param>
        /// <param name="configuration">The configuration.</param>
        public ElasticsearchHealthCheck(IElasticClient elasticClient, IConfiguration configuration)
        {
            _elasticClient = elasticClient;
            _configuration = configuration;
            _configuration.GetReloadToken().RegisterChangeCallback(_ => SetupConfig(), null);
        }

        private void SetupConfig()
        {
            _semaphore.Wait();
            _indices = _configuration
                .GetSection("Elasticsearch:Indices")
                .Get<KeyValuePair<string, string>[]>()
                .Select(i => i.Value)
                .ToArray();
            _timeout = _configuration
                .GetSection("Elasticsearch:Healthcheck:Timeout")
                .Get<TimeSpan>();
            _semaphore.Release();
        }

        /// <inheritdoc/>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);

                var healthResult = await _elasticClient.Cluster
                    .HealthAsync(
                        _indices,
                        req => req.Timeout(_timeout),
                        cancellationToken
                    );
                if (healthResult.IsValid && healthResult.Status == Health.Green)
                {
                    return HealthCheckResult.Healthy();
                }
                else if (healthResult.IsValid && healthResult.Status == Health.Yellow)
                {
                    return HealthCheckResult.Degraded(exception: healthResult.OriginalException);
                }
                else
                {
                    return HealthCheckResult.Unhealthy(exception: healthResult.OriginalException);
                }
            }
            catch (Exception e)
            {
                return HealthCheckResult.Unhealthy(exception: e);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
