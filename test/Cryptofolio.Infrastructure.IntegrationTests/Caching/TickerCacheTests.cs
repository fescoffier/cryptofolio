using Cryptofolio.Infrastructure.Caching;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Infrastructure.IntegrationTests.Caching
{
    public class TickerCacheTests : TestBase
    {
        private readonly FakeTickerCache _cache;

        public TickerCacheTests()
        {
            _cache = ResolveServiceFromScope<FakeTickerCache>();
        }

        protected override void ConfigureTestServices(IServiceCollection services) => services
            .AddSingleton(ConnectionMultiplexer.Connect(Configuration.GetConnectionString("Redis")))
            .AddSingleton(p => p.GetRequiredService<ConnectionMultiplexer>().GetDatabase())
            .AddSingleton<FakeTickerCache>();

        [Fact]
        public async Task Test()
        {
            // Setup
            var tickers = new[]
            {
                new Ticker
                {
                    Pair = "a/b",
                    Timestamp = DateTimeOffset.UtcNow,
                    Value = 1
                },
                new Ticker
                {
                    Pair = "c/d",
                    Timestamp = DateTimeOffset.UtcNow,
                    Value = 2
                }
            };

            // Act
            await _cache.StoreTickersAsync(tickers);
            var storedTickers = await _cache.GetTickersAsync("a/b", "c/d");

            // Assert
            storedTickers.Should().BeEquivalentTo(tickers);
        }

        private class FakeTickerCache : TickerCache
        {
            protected override string HashKey => "Fake";

            public FakeTickerCache(IDatabase database) : base(database)
            {
            }
        }
    }
}
