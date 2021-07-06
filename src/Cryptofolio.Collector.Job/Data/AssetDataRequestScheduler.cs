using Cryptofolio.Infrastructure;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Collector.Job.Data
{
    public class AssetDataRequestScheduler : BackgroundService
    {
        private readonly CryptofolioContext 

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
