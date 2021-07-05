using Cryptofolio.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Provides entity configuration for <see cref="AssetTicker"/>.
    /// </summary>
    public class AssetTickerEntityConfiguration : IEntityTypeConfiguration<AssetTicker>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<AssetTicker> builder)
        {
            builder.ToTable("asset_ticker");
            builder.Property<string>("asset_id");
            builder.HasKey("asset_id", nameof(AssetTicker.Timestamp));
            builder.HasOne(p => p.Asset).WithMany().HasForeignKey("asset_id");
            builder.Property(p => p.Timestamp).HasColumnName("timestamp");
            builder.Property(p => p.Value).HasColumnName("value");
            builder.Property(p => p.VsCurrency).HasMaxLength(10).HasColumnName("vs_currency");
            builder.HasIndex(p => p.Timestamp);
            builder.HasIndex(p => p.VsCurrency);
        }
    }
}
