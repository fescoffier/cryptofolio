using Cryptofolio.Api.Commands;
using Cryptofolio.Api.Controllers;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Entities;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Api.Tests.Controllers
{
    public class WalletControllerTests
    {
        private readonly CryptofolioContext _context;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly WalletController _controller;

        public WalletControllerTests()
        {
            _context = new(new DbContextOptionsBuilder<CryptofolioContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            _mediatorMock = new();
            _controller = new(_mediatorMock.Object, _context)
            {
                ControllerContext = new()
                {
                    HttpContext = TestContext.Instance.HttpContext
                }
            };
        }

        [Fact]
        public async Task Get_Id_Test()
        {
            // Setup
            var requestContext = TestContext.Instance.RequestContext;
            var wallet = new Wallet
            {
                Id = Guid.NewGuid().ToString(),
                UserId = TestContext.Instance.UserId
            };
            _context.Wallets.Add(wallet);
            _context.SaveChanges();
            var cancellationToken = CancellationToken.None;

            // Act
            var w = await _controller.Get(wallet.Id, requestContext, cancellationToken);

            // Assert
            w.Should().BeEquivalentTo(wallet);
        }

        [Fact]
        public async Task Get_Id_NotFound_Test()
        {
            // Setup
            var requestContext = TestContext.Instance.RequestContext;
            var wallet = new Wallet
            {
                Id = Guid.NewGuid().ToString(),
                UserId = TestContext.Instance.UserId
            };
            _context.Wallets.Add(wallet);
            _context.SaveChanges();
            var cancellationToken = CancellationToken.None;

            // Act
            var w = await _controller.Get(Guid.NewGuid().ToString(), requestContext, cancellationToken);

            // Assert
            w.Should().BeNull();
        }

        [Fact]
        public async Task Get_Id_NotOwned_Test()
        {
            // Setup
            var requestContext = TestContext.Instance.RequestContext;
            var wallet = new Wallet
            {
                Id = Guid.NewGuid().ToString(),
                UserId = Guid.NewGuid().ToString()
            };
            _context.Wallets.Add(wallet);
            _context.SaveChanges();
            var cancellationToken = CancellationToken.None;

            // Act
            var w = await _controller.Get(wallet.Id, requestContext, cancellationToken);

            // Assert
            w.Should().BeNull();
        }

        [Fact]
        public async Task Get_List_Test()
        {
            // Setup
            var requestContext = TestContext.Instance.RequestContext;
            var wallet = new Wallet
            {
                Id = Guid.NewGuid().ToString(),
                UserId = TestContext.Instance.UserId
            };
            _context.Wallets.Add(wallet);
            _context.SaveChanges();
            var cancellationToken = CancellationToken.None;

            // Act
            var ws = await _controller.Get(requestContext, cancellationToken);

            // Assert
            ws.Should().HaveCount(1).And.ContainEquivalentOf(wallet);
        }

        [Fact]
        public async Task Get_List_Empty_Test()
        {
            // Setup
            var requestContext = TestContext.Instance.RequestContext;
            var wallet = new Wallet
            {
                Id = Guid.NewGuid().ToString(),
                UserId = TestContext.Instance.UserId
            };
            _context.Wallets.Add(wallet);
            _context.SaveChanges();
            var cancellationToken = CancellationToken.None;

            // Act
            var ws = await _controller.Get(requestContext, cancellationToken);

            // Assert
            ws.Should().HaveCount(1).And.ContainEquivalentOf(wallet);
        }

        [Fact]
        public async Task Create_Test()
        {
            // Setup
            var command = new CreateWalletCommand();
            var commandResult = CommandResult<Wallet>.Success(new() { Id = Guid.NewGuid().ToString() });
            var cancellationToken = CancellationToken.None;
            _mediatorMock.Setup(m => m.Send(command, cancellationToken)).ReturnsAsync(commandResult);

            // Act
            var result = await _controller.Create(command, cancellationToken);

            // Assert
            result
                .Should()
                .BeOfType<CreatedAtActionResult>()
                .Which
                .Should()
                .BeEquivalentTo(new CreatedAtActionResult("Create", null, new { id = commandResult.Data.Id }, commandResult.Data));
        }

        [Fact]
        public async Task Create_InternalServerError_Test()
        {
            // Setup
            var command = new CreateWalletCommand();
            var commandResult = CommandResult<Wallet>.Failed();
            var cancellationToken = CancellationToken.None;
            _mediatorMock.Setup(m => m.Send(command, cancellationToken)).ReturnsAsync(commandResult);

            // Act
            var result = await _controller.Create(command, cancellationToken);

            // Assert
            result
                .Should()
                .BeOfType<ObjectResult>()
                .Which
                .Should()
                .BeEquivalentTo(new ObjectResult(commandResult) { StatusCode = StatusCodes.Status500InternalServerError });
        }

        [Fact]
        public async Task Update_Test()
        {
            // Setup
            var command = new UpdateWalletCommand();
            var commandResult = CommandResult.Success();
            var cancellationToken = CancellationToken.None;
            _mediatorMock.Setup(m => m.Send(command, cancellationToken)).ReturnsAsync(commandResult);

            // Act
            var result = await _controller.Update(command, cancellationToken);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Update_InternalServerError_Test()
        {
            // Setup
            var command = new UpdateWalletCommand();
            var commandResult = CommandResult.Failed();
            var cancellationToken = CancellationToken.None;
            _mediatorMock.Setup(m => m.Send(command, cancellationToken)).ReturnsAsync(commandResult);

            // Act
            var result = await _controller.Update(command, cancellationToken);

            // Assert
            result
                .Should()
                .BeOfType<ObjectResult>()
                .Which
                .Should()
                .BeEquivalentTo(new ObjectResult(commandResult) { StatusCode = StatusCodes.Status500InternalServerError });
        }

        [Fact]
        public async Task Delete_Test()
        {
            // Setup
            var requestContext = TestContext.Instance.RequestContext;
            var id = Guid.NewGuid().ToString();
            var commandResult = CommandResult.Success();
            var cancellationToken = CancellationToken.None;
            _mediatorMock.Setup(m => m.Send(It.Is<DeleteWalletCommand>(c => c.Id == id), cancellationToken)).ReturnsAsync(commandResult);

            // Act
            var result = await _controller.Delete(id, requestContext, cancellationToken);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Delete_InternalServerError_Test()
        {
            // Setup
            var requestContext = TestContext.Instance.RequestContext;
            var id = Guid.NewGuid().ToString();
            var commandResult = CommandResult.Failed();
            var cancellationToken = CancellationToken.None;
            _mediatorMock.Setup(m => m.Send(It.Is<DeleteWalletCommand>(c => c.Id == id), cancellationToken)).ReturnsAsync(commandResult);

            // Act
            var result = await _controller.Delete(id, requestContext, cancellationToken);

            // Assert
            result
                .Should()
                .BeOfType<ObjectResult>()
                .Which
                .Should()
                .BeEquivalentTo(new ObjectResult(commandResult) { StatusCode = StatusCodes.Status500InternalServerError });
        }
    }
}
