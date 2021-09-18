using Cryptofolio.App.Balances;
using Cryptofolio.App.Hubs;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Balances;
using Cryptofolio.Infrastructure.Caching;
using Cryptofolio.Infrastructure.Entities;
using Cryptofolio.Infrastructure.TestsCommon;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.App.IntegrationTests.Balances
{
    public class ComputeWalletBalanceResponseHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly ComputeWalletBalanceResponseHandler _handler;
        private readonly CryptofolioContext _context;
        private readonly AssetTickerCache _tickerCache;

        private TestData Data => _factory.Data;

        public ComputeWalletBalanceResponseHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<ComputeWalletBalanceResponseHandler>();
            _context = _scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            _tickerCache = _scope.ServiceProvider.GetRequiredService<AssetTickerCache>();
        }

        [Fact]
        public async Task Handle_Test()
        {
            // Setup
            _context.Wallets.Add(Data.Wallet1);
            _context.SaveChanges();
            await _tickerCache.StoreTickersAsync(
                new Ticker
                {
                    Pair = new(Data.BTC.Symbol, Data.USD.Code),
                    Timestamp = DateTimeOffset.UtcNow,
                    Value = Data.BTC_USD_Ticker.Value
                }
            );
            Data.BTC.CurrentValue = Data.BTC_USD_Ticker.Value;
            Wallet receivedWallet = null;

            var uriBuilder = new UriBuilder(_factory.Server.BaseAddress)
            {
                Path = DashboardHub.Endpoint
            };
            await using var connection = new HubConnectionBuilder()
                .WithUrl(
                    uriBuilder.Uri,
                    options => options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler()
                )
                .Build();
            connection.On<Wallet>(nameof(IDashboardClient.WalletBalanceChanged), wallet =>
            {
                receivedWallet = wallet;
            });
            var response = new ComputeWalletBalanceResponse
            {
                WalletId = Data.Wallet1.Id
            };
            var cancellationToken = CancellationToken.None;

            // Act
            await connection.StartAsync(cancellationToken);
            await _handler.Handle(response, cancellationToken);
            await Task.Delay(1000);
            await connection.StopAsync(cancellationToken);

            // Assert
            receivedWallet.Should().BeEquivalentTo(Data.Wallet1);
        }
    }
}
