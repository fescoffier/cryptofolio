using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Provides entity configuration for <see cref="Holding"/>.
    /// </summary>
    public class HoldingEntityTypeConfiguration : IEntityTypeConfiguration<Holding>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Holding> builder)
        {
            builder.ToTable("holding");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasMaxLength(36).HasColumnName("id");
            builder.Property(p => p.Amount).HasColumnName("amount");
            builder.Property<string>("wallet_id");
            builder.HasOne(p => p.Wallet).WithMany(p => p.Holdings).HasForeignKey("wallet_id").IsRequired();
            builder.Property<string>("asset_id");
            builder.HasOne(p => p.Asset).WithMany().HasForeignKey("asset_id").IsRequired();
            builder.HasIndex("wallet_id", "asset_id").IsUnique();
        }
    }
}
