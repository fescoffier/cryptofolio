using Cryptofolio.Collector.Job.Data;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Data;
using Cryptofolio.Infrastructure.Entities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Collector.Job.IntegrationTests.Data
{
    public class AssetTickerDataRequestSchedulerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory _factory;
        private readonly AssetTickerDataRequestScheduler _scheduler;
        private readonly KafkaConsumerWrapper<string, AssetTickerDataRequest> _consumerWrapper;

        public AssetTickerDataRequestSchedulerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scheduler = factory.Services.GetRequiredService<AssetTickerDataRequestScheduler>();
            _consumerWrapper = factory.Services.GetRequiredService<KafkaConsumerWrapper<string, AssetTickerDataRequest>>();
        }

        [Fact]
        public async Task PrepareMessages_Test()
        {
            // Setup
            var settings = new[]
            {
                new Setting
                {
                    Key = Guid.NewGuid().ToString(),
                    Group = $"{typeof(AssetTickerDataRequestScheduler).FullName}:Ids",
                    Value = "bitcoin"
                },
                new Setting
                {
                    Key = Guid.NewGuid().ToString(),
                    Group = $"{typeof(AssetTickerDataRequestScheduler).FullName}:Ids",
                    Value = "ethereum"
                },
                new Setting
                {
                    Key = Guid.NewGuid().ToString(),
                    Group = $"{typeof(AssetTickerDataRequestScheduler).FullName}:VsCurrencies",
                    Value = "usd"
                },
                new Setting
                {
                    Key = Guid.NewGuid().ToString(),
                    Group = $"{typeof(AssetTickerDataRequestScheduler).FullName}:VsCurrencies",
                    Value = "eur"
                }
            };
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            context.Settings.AddRange(settings);
            context.SaveChanges();

            // Act
            await _scheduler.StartAsync(CancellationToken.None);
            await Task.Delay(TimeSpan.FromSeconds(65));
            await _scheduler.StopAsync(CancellationToken.None);

            // Asset
            _consumerWrapper.Consumer.Subscribe(_consumerWrapper.Options.Topic);
            var cr = _consumerWrapper.Consumer.Consume();
            _consumerWrapper.Consumer.Unsubscribe();
            cr.Message.Value
                .Should()
                .BeEquivalentTo(new AssetTickerDataRequest
                {
                    Ids = new[]
                    {
                        "bitcoin",
                        "ethereum"
                    },
                    VsCurrencies = new[]
                    {
                        "usd",
                        "eur"
                    }
                },
                options => options.Excluding(p => p.TraceIdentifier).Excluding(p => p.Date));
        }
    }
}
