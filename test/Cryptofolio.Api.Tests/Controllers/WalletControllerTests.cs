using Cryptofolio.Api.Commands;
using Cryptofolio.Api.Controllers;
using Cryptofolio.Infrastructure.Entities;
using FluentAssertions;
using IdentityModel;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Api.Tests.Controllers
{
    public class WalletControllerTests
    {
        [Fact]
        public async Task Create_Test()
        {
            // Setup
            var mediatorMock = new Mock<IMediator>();
            var controller = new WalletController(mediatorMock.Object, null)
            {
                ControllerContext = new()
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new(new ClaimsIdentity(new[]
                        {
                            new Claim(JwtClaimTypes)
                        }))
                    }
                }
            };
            var command = new CreateWalletCommand
            {
                
            };
            var commandResult = CommandResult<Wallet>.Success(new() { Id = Guid.NewGuid().ToString() });
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await controller.Create(command, cancellationToken);

            // Assert
            result
                .Should()
                .BeOfType<CreatedAtActionResult>()
                .Which
                .Should()
                .BeEquivalentTo(new CreatedAtActionResult("Create", "Wallet", new { id = commandResult.Data.Id }, commandResult.Data));
        }
    }
}
