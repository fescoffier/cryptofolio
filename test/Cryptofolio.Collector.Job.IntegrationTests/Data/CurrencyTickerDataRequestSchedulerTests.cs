using Cryptofolio.Collector.Job.Data;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Data;
using Cryptofolio.Infrastructure.Entities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Collector.Job.IntegrationTests.Data
{
    public class CurrencyTickerDataRequestSchedulerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory _factory;
        private readonly CurrencyTickerDataRequestScheduler _scheduler;
        private readonly KafkaConsumerWrapper<string, CurrencyTickerDataRequest> _consumerWrapper;

        public CurrencyTickerDataRequestSchedulerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scheduler = factory.Services.GetRequiredService<CurrencyTickerDataRequestScheduler>();
            _consumerWrapper = factory.Services.GetRequiredService<KafkaConsumerWrapper<string, CurrencyTickerDataRequest>>();
        }

        [Fact]
        public async Task PrepareMessages_Test()
        {
            // Setup
            var settings = new[]
            {
                new Setting
                {
                    Key = $"Data:Schedules:{typeof(CurrencyTickerDataRequest).FullName}:Currency:0",
                    Group = $"{typeof(CurrencyTickerDataRequestScheduler).FullName}:Currency",
                    Value = "usd"
                },
                new Setting
                {
                    Key = $"Data:Schedules:{typeof(CurrencyTickerDataRequest).FullName}:Currency:0:VsCurrencies:0",
                    Group = $"{typeof(CurrencyTickerDataRequestScheduler).FullName}:VsCurrencies",
                    Value = "eur"
                },
                new Setting
                {
                    Key = $"Data:Schedules:{typeof(CurrencyTickerDataRequest).FullName}:Currency:0:VsCurrencies:1",
                    Group = $"{typeof(CurrencyTickerDataRequestScheduler).FullName}:VsCurrencies",
                    Value = "jpy"
                },
                new Setting
                {
                    Key = $"Data:Schedules:{typeof(CurrencyTickerDataRequest).FullName}:Currency:1",
                    Group = $"{typeof(CurrencyTickerDataRequestScheduler).FullName}:Currency",
                    Value = "eur"
                },
                new Setting
                {
                    Key = $"Data:Schedules:{typeof(CurrencyTickerDataRequest).FullName}:Currency:1:VsCurrencies:0",
                    Group = $"{typeof(CurrencyTickerDataRequestScheduler).FullName}:VsCurrencies",
                    Value = "usd"
                },
                new Setting
                {
                    Key = $"Data:Schedules:{typeof(CurrencyTickerDataRequest).FullName}:Currency:1:VsCurrencies:1",
                    Group = $"{typeof(CurrencyTickerDataRequestScheduler).FullName}:VsCurrencies",
                    Value = "jpy"
                }
            };
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            context.Settings.AddRange(settings);
            context.SaveChanges();

            // Act
            await _scheduler.StartAsync(CancellationToken.None);
            await Task.Delay(TimeSpan.FromSeconds(60));
            await _scheduler.StopAsync(CancellationToken.None);

            // Asset
            _consumerWrapper.Consumer.Subscribe(_consumerWrapper.Options.Topic);
            var messages = new List<CurrencyTickerDataRequest>
            {
                _consumerWrapper.Consumer.Consume().Message.Value,
                _consumerWrapper.Consumer.Consume().Message.Value
            };
            _consumerWrapper.Consumer.Unsubscribe();
            messages
                .Should()
                .BeEquivalentTo(new[]
                {
                    new CurrencyTickerDataRequest
                    {
                        Currency = "usd",
                        VsCurrencies = new[]
                        {
                            "eur",
                            "jpy"
                        }
                    },
                    new CurrencyTickerDataRequest
                    {
                        Currency = "eur",
                        VsCurrencies = new[]
                        {
                            "usd",
                            "jpy"
                        }
                    }
                },
                options => options.Excluding(p => p.TraceIdentifier).Excluding(p => p.Date));
        }
    }
}
