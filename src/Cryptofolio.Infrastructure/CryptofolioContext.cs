using Microsoft.EntityFrameworkCore;

namespace Cryptofolio.Infrastructure
{
    /// <summary>
    /// Provides a wrapper to access database objects.
    /// </summary>
    public class CryptofolioContext : DbContext
    {
        /// <summary>
        /// Creates a new instance of <see cref="CryptofolioContext"/>.
        /// </summary>
        /// <param name="options">The options that configures the context.</param>
        public CryptofolioContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CryptofolioContext).Assembly);
        }
    }
}
