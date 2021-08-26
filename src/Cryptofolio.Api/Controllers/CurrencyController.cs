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
    [Route("/currencies")]
    public class CurrencyController : ControllerBase
    {
        private readonly CryptofolioContext _context;

        public CurrencyController(CryptofolioContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public Task<Currency> Get(string id, CancellationToken cancellationToken) =>
            _context.Currencies.AsNoTracking().SingleOrDefaultAsync(e => e.Id == id, cancellationToken);

        [HttpGet]
        public Task<List<Currency>> Get(CancellationToken cancellationToken) =>
            _context.Currencies.AsNoTracking().OrderBy(e => e.Code).ToListAsync(cancellationToken);
    }
}
