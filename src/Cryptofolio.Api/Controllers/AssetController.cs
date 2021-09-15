using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/assets")]
    public class AssetController : ControllerBase
    {
        private readonly CryptofolioContext _context;

        public AssetController(CryptofolioContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public Task<Asset> Get(string id, CancellationToken cancellationToken) =>
            _context.Assets.AsNoTracking().SingleOrDefaultAsync(e => e.Id == id, cancellationToken);

        [HttpGet("{id}/tickers/{vs_currency}/latest")]
        public Task<AssetTicker> GetLatestTicker(string id, [FromRoute(Name = "vs_currency")] string vsCurrency, CancellationToken cancellationToken) =>
            _context.AssetTickers
                .AsNoTracking()
                .Include(t => t.Asset)
                .Include(t => t.VsCurrency)
                .Where(t => t.Asset.Id == id && t.VsCurrency.Code == vsCurrency)
                .OrderByDescending(t => t.Timestamp)
                .FirstOrDefaultAsync(cancellationToken);

        [HttpGet("{id}/tickers/{vs_currency}/{timestamp:datetime}")]
        public Task<AssetTicker> GetTicker(string id, [FromRoute(Name = "vs_currency")] string vsCurrency, DateTimeOffset timestamp, CancellationToken cancellationToken) =>
            _context.AssetTickers
                .AsNoTracking()
                .Include(t => t.Asset)
                .Include(t => t.VsCurrency)
                .Where(t => t.Asset.Id == id && t.VsCurrency.Code == vsCurrency && t.Timestamp <= timestamp)
                .OrderByDescending(t => t.Timestamp)
                .FirstOrDefaultAsync(cancellationToken);

        [HttpGet]
        public Task<List<Asset>> Get(CancellationToken cancellationToken) =>
            _context.Assets.AsNoTracking().OrderBy(e => e.Name).ToListAsync(cancellationToken);

        [HttpGet("sources")]
        public IEnumerable<string> Sources() => InfrastructureConstants.Transactions.Sources.All;

        [HttpGet("destinations")]
        public IEnumerable<string> Destinations() => InfrastructureConstants.Transactions.Destinations.All;
    }
}
