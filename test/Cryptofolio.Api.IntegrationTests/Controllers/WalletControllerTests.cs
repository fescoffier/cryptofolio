using Cryptofolio.Api.Commands;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly TestData _data;

        public WalletControllerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _data = factory.Data;

            using var scope = _factory.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<CryptofolioContext>().Database.ExecuteSqlRaw("delete from \"data\".\"wallet\"");
        }

        [Fact]
        public async Task Create_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            var command = new CreateWalletCommand
            {
                Name = _data.Wallet1.Name,
                Description = _data.Wallet1.Description
            };

            // Act
            var response = await client.PostAsJsonAsync("/wallets", command);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status201Created);
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
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
                Name = _data.Wallet1.Name,
                Description = _data.Wallet1.Description
            };
            // Change the test data user id with a value length greater than 36 characters, triggers a database exception.
            _data.ChangUserId(new string(Enumerable.Repeat('A', 37).ToArray()));

            // Act
            var response = await client.PostAsJsonAsync("/wallets", command);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            document.RootElement.GetProperty("errors").EnumerateArray().Should().HaveCount(1);
            document.RootElement.GetProperty("errors").EnumerateArray().First().GetString().Should().Be(CommandConstants.Wallet.Errors.CreateError);
            document.RootElement.GetProperty("succeeded").GetBoolean().Should().BeFalse();
            _data.RestoreUserId();
        }

        [Fact]
        public async Task Update_Test()
        {
            // Setup
            var client = _factory.CreateClient();
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
                context.Wallets.Add(_data.Wallet2);
                context.SaveChanges();
            }
            var command = new UpdateWalletCommand
            {
                Id = _data.Wallet2.Id,
                Name = _data.Wallet2.Name + " updated",
                Description = _data.Wallet2.Description
            };

            // Act
            var response = await client.PutAsJsonAsync("/wallets", command);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
                var wallet = context.Wallets.Single(w => w.Id == _data.Wallet2.Id);
                wallet.Name.Should().Be(_data.Wallet2.Name + " updated");
                wallet.Description.Should().Be(_data.Wallet2.Description);
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
                Id = _data.Wallet2.Id,
                Name = _data.Wallet2.Name,
                Description = _data.Wallet2.Description
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
                context.Wallets.AddRange(_data.Wallet1, _data.Wallet2, _data.Wallet3);
                context.SaveChanges();
            }

            // Act
            var response = await client.PutAsync($"/wallets/{_data.Wallet2.Id}/select", null);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
                var wallets = context.Wallets.ToList();
                wallets.Single(w => w.Id == _data.Wallet1.Id).Selected.Should().BeFalse();
                wallets.Single(w => w.Id == _data.Wallet2.Id).Selected.Should().BeTrue();
                wallets.Single(w => w.Id == _data.Wallet3.Id).Selected.Should().BeFalse();
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
                context.Wallets.AddRange(_data.Wallet1, _data.Wallet2, _data.Wallet3);
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
                wallets.Single(w => w.Id == _data.Wallet1.Id).Selected.Should().BeTrue();
                wallets.Single(w => w.Id == _data.Wallet2.Id).Selected.Should().BeFalse();
                wallets.Single(w => w.Id == _data.Wallet3.Id).Selected.Should().BeFalse();
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
                context.Wallets.Add(_data.Wallet3);
                context.SaveChanges();
            }

            // Act
            var response = await client.DeleteAsync($"/wallets/{_data.Wallet3.Id}");

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CryptofolioContext>();
                context.Wallets.SingleOrDefault(w => w.Id == _data.Wallet3.Id).Should().BeNull();
            }
        }

        [Fact]
        public async Task Delete_InternalServerError_Test()
        {
            // Setup
            var client = _factory.CreateClient();

            // Act
            var response = await client.DeleteAsync($"/wallets/{_data.Wallet3.Id}");

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            document.RootElement.GetProperty("errors").EnumerateArray().Should().HaveCount(1);
            document.RootElement.GetProperty("errors").EnumerateArray().First().GetString().Should().Be(CommandConstants.Wallet.Errors.DeleteError);
            document.RootElement.GetProperty("succeeded").GetBoolean().Should().BeFalse();
        }
    }
}
