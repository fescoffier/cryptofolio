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
    [Route("/exchanges")]
    public class ExchangeController : ControllerBase
    {
        private readonly CryptofolioContext _context;

        public ExchangeController(CryptofolioContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public Task<Exchange> Get(string id, CancellationToken cancellationToken) =>
            _context.Exchanges.AsNoTracking().SingleOrDefaultAsync(e => e.Id == id, cancellationToken);

        [HttpGet]
        public Task<List<Exchange>> Get(CancellationToken cancellationToken) =>
            _context.Exchanges.AsNoTracking().OrderBy(e => e.Name).ToListAsync(cancellationToken);
    }
}
