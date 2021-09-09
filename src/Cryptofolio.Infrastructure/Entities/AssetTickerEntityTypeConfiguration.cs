using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Provides entity configuration for <see cref="AssetTicker"/>.
    /// </summary>
    public class AssetTickerEntityTypeConfiguration : IEntityTypeConfiguration<AssetTicker>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<AssetTicker> builder)
        {
            builder.ToTable("asset_ticker");
            builder.Property<string>("asset_id");
            builder.Property<string>("vs_currency_id");
            builder.HasKey("asset_id", "vs_currency_id", nameof(AssetTicker.Timestamp));
            builder.HasOne(p => p.Asset).WithMany().HasForeignKey("asset_id").IsRequired();
            builder.HasOne(p => p.VsCurrency).WithMany().HasForeignKey("vs_currency_id").IsRequired();
            builder.Property(p => p.Timestamp).HasColumnName("timestamp").IsRequired();
            builder.Property(p => p.Value).HasColumnName("value").IsRequired();
            builder.HasIndex(p => p.Timestamp);
        }
    }
}
