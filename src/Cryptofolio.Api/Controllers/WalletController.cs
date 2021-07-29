using Cryptofolio.Api.Commands;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/wallets")]
    public class WalletController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly CryptofolioContext _context;

        public WalletController(IMediator mediator, CryptofolioContext context)
        {
            _mediator = mediator;
            _context = context;
        }

        [HttpGet("{id}")]
        public Task<Wallet> Get(string id, CancellationToken cancellationToken) =>
            _context.Wallets.AsNoTracking().SingleOrDefaultAsync(w => w.Id == id, cancellationToken);

        [HttpGet]
        public Task<List<Wallet>> Get(CancellationToken cancellationToken) =>
            _context.Wallets.AsNoTracking().Where(w => w.UserId == User.Identity.Name).ToListAsync(cancellationToken);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWalletCommand command, CancellationToken cancellationToken)
        {
            command.EnsureTraceability(HttpContext);

            var result = await _mediator.Send(command, cancellationToken);
            if (result.Succeeded)
            {
                return CreatedAtAction(nameof(Create), new { id = result.Data.Id }, result.Data);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateWalletCommand command, CancellationToken cancellationToken)
        {
            command.EnsureTraceability(HttpContext);

            var result = await _mediator.Send(command, cancellationToken);
            if (result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Update(string id, CancellationToken cancellationToken)
        {
            var command = new DeleteWalletCommand
            {
                Id = id
            };
            command.EnsureTraceability(HttpContext);

            var result = await _mediator.Send(command, cancellationToken);
            if (result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }
        }
    }
}
