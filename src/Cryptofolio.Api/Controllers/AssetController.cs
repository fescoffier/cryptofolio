using Cryptofolio.Infrastructure;
using Cryptofolio.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet]
        public Task<List<Asset>> Get(CancellationToken cancellationToken) =>
            _context.Assets.AsNoTracking().OrderBy(e => e.Name).ToListAsync(cancellationToken);

        [HttpGet("sources")]
        public IEnumerable<string> Sources() => InfrastructureConstants.Transactions.Sources.All;

        [HttpGet("destinations")]
        public IEnumerable<string> Destinations() => InfrastructureConstants.Transactions.Destinations.All;
    }
}
