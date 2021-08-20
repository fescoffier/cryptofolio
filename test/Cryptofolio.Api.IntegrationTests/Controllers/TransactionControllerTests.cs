using Cryptofolio.Api.Commands;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Api.IntegrationTests.Controllers
{
    public class TransactionControllerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory _factory;

        private TestData Data => _factory.Data;

        public TransactionControllerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            factory.PurgeData();
        }

        [Fact]
        public async Task Get_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            context.Transactions.Add(Data.Transaction1);
            context.SaveChanges();

            // Act
            var transaction = await client.GetFromJsonAsync<BuyOrSellTransaction>($"/transactions/{Data.Transaction1.Id}");

            // Assert
            transaction.Should().BeEquivalentTo(Data.Transaction1, options => options.Excluding(m => m.Date));
            transaction.Date.Should().BeCloseTo(Data.Transaction1.Date, precision: 1);
        }

        [Fact]
        public async Task Get_List_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            context.Transactions.AddRange(Data.Transaction1, Data.Transaction2, Data.Transaction3, Data.Transaction4);
            context.SaveChanges();

            // Act
            var transactions = await client.GetFromJsonAsync<List<BuyOrSellTransaction>>("/transactions?skip=1&take=2");

            // Assert
            transactions.Should().HaveCount(2);
            transactions.Should().ContainEquivalentOf(Data.Transaction2, options => options.Excluding(m => m.Date));
            transactions.Single(t => t.Id == Data.Transaction2.Id).Date.Should().BeCloseTo(Data.Transaction2.Date, precision: 1);
            transactions.Should().ContainEquivalentOf(Data.Transaction3, options => options.Excluding(m => m.Date));
            transactions.Single(t => t.Id == Data.Transaction3.Id).Date.Should().BeCloseTo(Data.Transaction3.Date, precision: 1);
        }

        [Fact]
        public async Task Get_ByWalletList_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            context.Transactions.AddRange(Data.Transaction1, Data.Transaction2, Data.Transaction3, Data.Transaction4);
            context.SaveChanges();

            // Act
            var transactions = await client.GetFromJsonAsync<List<BuyOrSellTransaction>>($"/transactions?wallet_id={Data.Wallet1.Id}");

            // Assert
            transactions.Should().HaveCount(2);
            transactions.Should().ContainEquivalentOf(Data.Transaction1, options => options.Excluding(m => m.Date));
            transactions.Single(t => t.Id == Data.Transaction1.Id).Date.Should().BeCloseTo(Data.Transaction1.Date, precision: 1);
            transactions.Should().ContainEquivalentOf(Data.Transaction2, options => options.Excluding(m => m.Date));
            transactions.Single(t => t.Id == Data.Transaction2.Id).Date.Should().BeCloseTo(Data.Transaction2.Date, precision: 1);
        }

        [Fact]
        public async Task Create_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            context.Wallets.Add(Data.Transaction1.Wallet);
            context.Assets.Add(Data.Transaction1.Asset);
            context.Exchanges.Add(Data.Transaction1.Exchange);
            context.SaveChanges();
            var command = new CreateTransactionCommand
            {
                Type = CommandConstants.Transaction.Types.Buy,
                Date = Data.Transaction1.Date,
                WalletId = Data.Transaction1.Wallet.Id,
                AssetId = Data.Transaction1.Asset.Id,
                ExchangeId = Data.Transaction1.Exchange.Id,
                Currency = Data.Transaction1.Currency,
                Price = Data.Transaction1.Price,
                Qty = Data.Transaction1.Qty,
                Note = Data.Transaction1.Note
            };

            // Act
            var response = await client.PostAsJsonAsync("/transactions", command);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status201Created);
            var createdTransaction = await response.Content.ReadFromJsonAsync<BuyOrSellTransaction>();
            var transaction = context.Transactions
                .Include(t => t.Wallet)
                .Include(t => t.Asset)
                .Include(t => t.Exchange)
                .Single(w => w.Id == createdTransaction.Id);
            createdTransaction.Should().BeEquivalentTo(transaction, options => options.Excluding(m => m.Date));
            createdTransaction.Date.Should().BeCloseTo(transaction.Date, precision: 1);
        }

        [Fact]
        public async Task Create_BadRequest_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            var command = new CreateTransactionCommand();

            // Act
            var response = await client.PostAsJsonAsync("/transactions", command);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task Create_InternalServerError_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            context.Wallets.Add(Data.Transaction1.Wallet);
            context.SaveChanges();
            var command = new CreateTransactionCommand
            {
                Type = CommandConstants.Transaction.Types.Buy,
                Date = Data.Transaction1.Date,
                WalletId = Data.Transaction1.Wallet.Id,
                AssetId = Data.Transaction1.Asset.Id,
                ExchangeId = Data.Transaction1.Exchange.Id,
                Currency = Data.Transaction1.Currency,
                Price = Data.Transaction1.Price,
                Qty = Data.Transaction1.Qty,
                Note = Data.Transaction1.Note
            };

            // Act
            var response = await client.PostAsJsonAsync("/transactions", command);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            document.RootElement.GetProperty("errors").EnumerateArray().Should().HaveCount(1);
            document.RootElement.GetProperty("errors").EnumerateArray().First().GetString().Should().Be(CommandConstants.Transaction.Errors.CreateError);
            document.RootElement.GetProperty("succeeded").GetBoolean().Should().BeFalse();
        }

        [Fact]
        public async Task Update_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
                context.Exchanges.Add(Data.Exchange2);
                context.Transactions.Add(Data.Transaction1);
                context.SaveChanges();
            }
            var command = new UpdateTransactionCommand
            {
                Id = Data.Transaction1.Id,
                Type = CommandConstants.Transaction.Types.Buy,
                Date = Data.Transaction1.Date.AddDays(1),
                ExchangeId = Data.Exchange2.Id,
                Currency = "eur",
                Price = 500,
                Qty = 20,
                Note = "Consectetur adipiscing elit"
            };

            // Act
            var response = await client.PutAsJsonAsync("/transactions", command);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
                var transaction = context.Transactions
                    .OfType<BuyOrSellTransaction>()
                    .Include(t => t.Exchange)
                    .Single(t => t.Id == Data.Transaction1.Id);
                transaction.Date.Should().BeCloseTo(command.Date, precision: 1);
                transaction.Exchange.Id.Should().Be(command.ExchangeId);
                transaction.Currency.Should().Be(command.Currency);
                transaction.Price.Should().Be(command.Price);
                transaction.Qty.Should().Be(command.Qty);
                transaction.Note.Should().Be(command.Note);
            }
        }

        [Fact]
        public async Task Update_BadRequest_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            var command = new UpdateTransactionCommand();

            // Act
            var response = await client.PutAsJsonAsync("/transactions", command);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task Update_InternalServerError_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            var command = new UpdateTransactionCommand
            {
                Id = Data.Transaction1.Id,
                Type = CommandConstants.Transaction.Types.Buy,
                Date = Data.Transaction1.Date.AddDays(1),
                ExchangeId = Data.Exchange2.Id,
                Currency = "eur",
                Price = 500,
                Qty = 20,
                Note = "Consectetur adipiscing elit"
            };

            // Act
            var response = await client.PutAsJsonAsync("/transactions", command);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            document.RootElement.GetProperty("errors").EnumerateArray().Should().HaveCount(1);
            document.RootElement.GetProperty("errors").EnumerateArray().First().GetString().Should().Be(CommandConstants.Transaction.Errors.UpdateError);
            document.RootElement.GetProperty("succeeded").GetBoolean().Should().BeFalse();
        }

        [Fact]
        public async Task Delete_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
                context.Transactions.Add(Data.Transaction1);
                context.SaveChanges();
            }

            // Act
            var response = await client.DeleteAsync($"/transactions/{Data.Transaction1.Id}");

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
                context.Transactions.SingleOrDefault(w => w.Id == Data.Transaction1.Id).Should().BeNull();
            }
        }

        [Fact]
        public async Task Delete_InternalServerError_Test()
        {
            // Setup
            var client = _factory.CreateClient();

            // Act
            var response = await client.DeleteAsync($"/transactions/{Data.Transaction1.Id}");

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            document.RootElement.GetProperty("errors").EnumerateArray().Should().HaveCount(1);
            document.RootElement.GetProperty("errors").EnumerateArray().First().GetString().Should().Be(CommandConstants.Transaction.Errors.DeleteError);
            document.RootElement.GetProperty("succeeded").GetBoolean().Should().BeFalse();
        }
    }
}
