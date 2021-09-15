using Cryptofolio.Api.Commands;
using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Caching;
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
        private readonly AssetTickerCache _tickerCache;

        private IQueryable<Wallet> Wallets => _context.Wallets
            .AsNoTracking()
            .Include(w => w.Currency)
            .Include(w => w.Holdings)
            .ThenInclude(w => w.Asset);

        public WalletController(
            IMediator mediator,
            CryptofolioContext context,
            AssetTickerCache tickerCache)
        {
            _mediator = mediator;
            _context = context;
            _tickerCache = tickerCache;
        }

        [HttpGet("{id}")]
        public Task<Wallet> Get(string id, [FromServices] RequestContext requestContext, CancellationToken cancellationToken) =>
            Wallets.SingleOrDefaultAsync(w => w.Id == id && w.UserId == requestContext.UserId, cancellationToken);

        [HttpGet]
        public async Task<List<Wallet>> Get([FromServices] RequestContext requestContext, CancellationToken cancellationToken)
        {
            var wallets = await Wallets
                .Where(w => w.UserId == requestContext.UserId)
                .OrderByDescending(w => w.Selected)
                .ThenByDescending(w => w.CurrentValue)
                .ToListAsync(cancellationToken);
            var tickers = await _tickerCache.GetTickersAsync(wallets
                .SelectMany(w => w.Holdings.Select(h => new TickerPair(h.Asset.Symbol, w.Currency.Code)))
                .Distinct()
                .ToArray()
            );
            foreach (var wallet in wallets)
            {
                foreach (var holding in wallet.Holdings)
                {
                    holding.Asset.CurrentValue = tickers
                        .Where(t => t.Pair.Left == holding.Asset.Symbol && t.Pair.Right == wallet.Currency.Code)
                        .Select(t => t.Value)
                        .FirstOrDefault();
                    // Avoids circular ref.
                    holding.Wallet = null;
                }
                wallet.Holdings = wallet.Holdings.OrderByDescending(h => h.CurrentValue).ToArray();
            }
            return wallets;
        }

        [HttpPost]
        [ServiceFilter(typeof(RequestContextActionFilter))]
        public async Task<IActionResult> Create(CreateWalletCommand command, CancellationToken cancellationToken)
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
        public async Task<IActionResult> Update(UpdateWalletCommand command, CancellationToken cancellationToken)
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

        [HttpPut("{id}/select")]
        public async Task<IActionResult> Select(string id, [FromServices] RequestContext requestContext, CancellationToken cancellationToken)
        {
            var command = new SetSelectedWalletCommand
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
