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
        public Task<Wallet> Get(string id, [FromServices] RequestContext requestContext, CancellationToken cancellationToken) =>
            _context.Wallets.AsNoTracking().SingleOrDefaultAsync(w => w.Id == id && w.UserId == requestContext.UserId, cancellationToken);

        [HttpGet]
        public Task<List<Wallet>> Get([FromServices] RequestContext requestContext, CancellationToken cancellationToken) =>
            _context.Wallets.AsNoTracking().Where(w => w.UserId == requestContext.UserId).ToListAsync(cancellationToken);

        [HttpPost]
        public async Task<IActionResult> Create(CreateWalletCommand command, [FromServices] RequestContext requestContext, CancellationToken cancellationToken)
        {
            command.RequestContext = requestContext;

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
        public async Task<IActionResult> Update(UpdateWalletCommand command, [FromServices] RequestContext requestContext, CancellationToken cancellationToken)
        {
            command.RequestContext = requestContext;

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
        public async Task<IActionResult> Delete(string id, [FromServices] RequestContext requestContext, CancellationToken cancellationToken)
        {
            var command = new DeleteWalletCommand
            {
                RequestContext = requestContext,
                Id = id
            };

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
