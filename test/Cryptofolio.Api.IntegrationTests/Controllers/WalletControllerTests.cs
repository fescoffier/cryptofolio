using Cryptofolio.Api.Commands;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Entities;
using Cryptofolio.Infrastructure.TestsCommon;
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
    public class WalletControllerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory _factory;

        private TestData Data => _factory.Data;

        public WalletControllerTests(WebApplicationFactory factory)
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
            context.Wallets.Add(Data.Wallet1);
            context.SaveChanges();

            // Act
            var wallet = await client.GetFromJsonAsync<Wallet>($"/wallets/{Data.Wallet1.Id}");

            // Assert
            wallet.Should().BeEquivalentTo(Data.Wallet1);
        }

        [Fact]
        public async Task Get_List_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            context.Wallets.AddRange(Data.Wallet1, Data.Wallet2, Data.Wallet3);
            context.SaveChanges();

            // Act
            var wallets = await client.GetFromJsonAsync<List<Wallet>>("/wallets");

            // Assert
            wallets.Should().HaveCount(3);
            wallets.Should().ContainEquivalentOf(Data.Wallet1);
            wallets.Should().ContainEquivalentOf(Data.Wallet2);
            wallets.Should().ContainEquivalentOf(Data.Wallet3);
        }

        [Fact]
        public async Task Create_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            var command = new CreateWalletCommand
            {
                Name = Data.Wallet1.Name,
                Description = Data.Wallet1.Description,
                CurrencyId = Data.Wallet1.Currency.Id
            };
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
            context.Currencies.Add(Data.USD);
            context.SaveChanges();

            // Act
            var response = await client.PostAsJsonAsync("/wallets", command);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status201Created);
            var createdWallet = await response.Content.ReadFromJsonAsync<Wallet>();
            var wallet = context.Wallets.Single(w => w.Id == createdWallet.Id);
            createdWallet.Should().BeEquivalentTo(wallet);
        }

        [Fact]
        public async Task Create_BadRequest_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            var command = new CreateWalletCommand();

            // Act
            var response = await client.PostAsJsonAsync("/wallets", command);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task Create_InternalServerError_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            var command = new CreateWalletCommand
            {
                Name = Data.Wallet1.Name,
                Description = Data.Wallet1.Description,
                CurrencyId = Data.Wallet1.Currency.Id
            };
            // Change the test data user id with a value length greater than 36 characters, triggers a database exception.
            Data.ChangUserId(new string(Enumerable.Repeat('A', 37).ToArray()));

            // Act
            var response = await client.PostAsJsonAsync("/wallets", command);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            document.RootElement.GetProperty("errors").EnumerateArray().Should().HaveCount(1);
            document.RootElement.GetProperty("errors").EnumerateArray().First().GetString().Should().Be(CommandConstants.Wallet.Errors.CreateError);
            document.RootElement.GetProperty("succeeded").GetBoolean().Should().BeFalse();
            Data.RestoreUserId();
        }

        [Fact]
        public async Task Update_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
                context.Wallets.Add(Data.Wallet2);
                context.SaveChanges();
            }
            var command = new UpdateWalletCommand
            {
                Id = Data.Wallet2.Id,
                Name = Data.Wallet2.Name + " updated",
                Description = Data.Wallet2.Description,
                CurrencyId = Data.Wallet2.Currency.Id
            };

            // Act
            var response = await client.PutAsJsonAsync("/wallets", command);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
                var wallet = context.Wallets.Include(w => w.Currency).Single(w => w.Id == Data.Wallet2.Id);
                wallet.Name.Should().Be(Data.Wallet2.Name + " updated");
                wallet.Description.Should().Be(Data.Wallet2.Description);
                wallet.Currency.Should().BeEquivalentTo(Data.Wallet2.Currency);
            }
        }

        [Fact]
        public async Task Update_BadRequest_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            var command = new UpdateWalletCommand();

            // Act
            var response = await client.PutAsJsonAsync("/wallets", command);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task Update_InternalServerError_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            var command = new UpdateWalletCommand
            {
                Id = Data.Wallet2.Id,
                Name = Data.Wallet2.Name,
                Description = Data.Wallet2.Description,
                CurrencyId = Data.Wallet1.Currency.Id
            };

            // Act
            var response = await client.PutAsJsonAsync("/wallets", command);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            document.RootElement.GetProperty("errors").EnumerateArray().Should().HaveCount(1);
            document.RootElement.GetProperty("errors").EnumerateArray().First().GetString().Should().Be(CommandConstants.Wallet.Errors.UpdateError);
            document.RootElement.GetProperty("succeeded").GetBoolean().Should().BeFalse();
        }

        [Fact]
        public async Task Select_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
                context.Wallets.AddRange(Data.Wallet1, Data.Wallet2, Data.Wallet3);
                context.SaveChanges();
            }

            // Act
            var response = await client.PutAsync($"/wallets/{Data.Wallet2.Id}/select", null);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
                var wallets = context.Wallets.ToList();
                wallets.Single(w => w.Id == Data.Wallet1.Id).Selected.Should().BeFalse();
                wallets.Single(w => w.Id == Data.Wallet2.Id).Selected.Should().BeTrue();
                wallets.Single(w => w.Id == Data.Wallet3.Id).Selected.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Select_InternalServerError_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
                context.Wallets.AddRange(Data.Wallet1, Data.Wallet2, Data.Wallet3);
                context.SaveChanges();
            }

            // Act
            var response = await client.PutAsync($"/wallets/none/select", null);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            document.RootElement.GetProperty("errors").EnumerateArray().Should().HaveCount(1);
            document.RootElement.GetProperty("errors").EnumerateArray().First().GetString().Should().Be(CommandConstants.Wallet.Errors.UpdateError);
            document.RootElement.GetProperty("succeeded").GetBoolean().Should().BeFalse();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
                var wallets = context.Wallets.ToList();
                wallets.Single(w => w.Id == Data.Wallet1.Id).Selected.Should().BeTrue();
                wallets.Single(w => w.Id == Data.Wallet2.Id).Selected.Should().BeFalse();
                wallets.Single(w => w.Id == Data.Wallet3.Id).Selected.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Delete_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
                context.Wallets.Add(Data.Wallet3);
                context.SaveChanges();
            }

            // Act
            var response = await client.DeleteAsync($"/wallets/{Data.Wallet3.Id}");

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
                context.Wallets.SingleOrDefault(w => w.Id == Data.Wallet3.Id).Should().BeNull();
            }
        }

        [Fact]
        public async Task Delete_InternalServerError_Test()
        {
            // Setup
            var client = _factory.CreateClient();

            // Act
            var response = await client.DeleteAsync($"/wallets/{Data.Wallet3.Id}");

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            document.RootElement.GetProperty("errors").EnumerateArray().Should().HaveCount(1);
            document.RootElement.GetProperty("errors").EnumerateArray().First().GetString().Should().Be(CommandConstants.Wallet.Errors.DeleteError);
            document.RootElement.GetProperty("succeeded").GetBoolean().Should().BeFalse();
        }
    }
}
