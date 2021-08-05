using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cryptofolio.App
{
    /// <summary>
    /// Provides a wrapper to access identity objects in database.
    /// </summary>
    public class IdentityContext : IdentityDbContext
    {
        /// <summary>
        /// Creates a new instance of <see cref="IdentityContext"/>.
        /// </summary>
        /// <param name="options">The options.</param>
        public IdentityContext(DbContextOptions options) : base(options)
        {
        }
    }
}
