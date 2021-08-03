using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Provides entity configuration for <see cref="Wallet"/>.
    /// </summary>
    public class WalletEntityTypeConfiguration : IEntityTypeConfiguration<Wallet>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.ToTable("wallet");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasMaxLength(36).HasColumnName("id");
            builder.Property(p => p.Name).IsRequired().HasMaxLength(250).HasColumnName("name");
            builder.Property(p => p.Description).HasColumnType("text").HasColumnName("description");
            builder.Property(p => p.UserId).IsRequired().HasMaxLength(36).HasColumnName("user_id");
        }
    }
}
