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
    [Route("/transactions")]
    public class TransactionController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly CryptofolioContext _context;

        private IQueryable<Transaction> Transactions => _context.Transactions
            .Include(t => t.Wallet)
            .Include(t => t.Asset)
            .Include(t => t.Exchange)
            .Include(nameof(BuyOrSellTransaction.Currency));

        public TransactionController(IMediator mediator, CryptofolioContext context)
        {
            _mediator = mediator;
            _context = context;
        }

        [HttpGet("{id}")]
        public Task<Transaction> Get(string id, [FromServices] RequestContext requestContext, CancellationToken cancellationToken) =>
            Transactions.AsNoTracking().SingleOrDefaultAsync(t => t.Id == id && t.Wallet.UserId == requestContext.UserId, cancellationToken);

        [HttpGet]
        public Task<List<Transaction>> Get(
            [FromQuery(Name = "wallet_id")] string walletId,
            int skip,
            int take,
            [FromServices] RequestContext requestContext,
            CancellationToken cancellationToken)
        {
            if (skip < 0 || take < 0)
            {
                return Task.FromResult(new List<Transaction>());
            }
            if (take == 0)
            {
                take = 20;
            }

            var query = Transactions.Where(t => t.Wallet.UserId == requestContext.UserId);
            if (!string.IsNullOrWhiteSpace(walletId))
            {
                query = query.Where(t => t.Wallet.Id == walletId);
            }

            return query
                .AsNoTracking()
                .OrderByDescending(t => t.Date)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        [HttpPost]
        [ServiceFilter(typeof(RequestContextActionFilter))]
        public async Task<IActionResult> Create(CreateTransactionCommand command, CancellationToken cancellationToken)
        {
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
        [ServiceFilter(typeof(RequestContextActionFilter))]
        public async Task<IActionResult> Update(UpdateTransactionCommand command, CancellationToken cancellationToken)
        {
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
            var command = new DeleteTransactionCommand
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
