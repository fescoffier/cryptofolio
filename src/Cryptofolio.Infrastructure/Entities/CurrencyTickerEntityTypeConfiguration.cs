using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Provides entity configuration for <see cref="CurrencyTicker"/>.
    /// </summary>
    public class CurrencyTickerEntityTypeConfiguration : IEntityTypeConfiguration<CurrencyTicker>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<CurrencyTicker> builder)
        {
            builder.ToTable("currency_ticker");
            builder.Property<string>("currency_id");
            builder.Property<string>("vs_currency_id");
            builder.HasKey("currency_id", "vs_currency_id", nameof(AssetTicker.Timestamp));
            builder.HasOne(p => p.Currency).WithMany().HasForeignKey("currency_id").IsRequired();
            builder.HasOne(p => p.VsCurrency).WithMany().HasForeignKey("vs_currency_id").IsRequired();
            builder.Property(p => p.Timestamp).HasColumnName("timestamp");
            builder.Property(p => p.Value).HasColumnName("value");
            builder.HasIndex(p => p.Timestamp);
        }
    }
}
