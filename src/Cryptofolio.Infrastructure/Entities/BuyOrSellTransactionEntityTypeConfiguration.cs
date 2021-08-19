using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Provides entity configuration for <see cref="BuyOrSellTransaction"/>.
    /// </summary>
    public class BuyOrSellTransactionEntityTypeConfiguration : IEntityTypeConfiguration<BuyOrSellTransaction>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<BuyOrSellTransaction> builder)
        {
            builder.HasBaseType<Transaction>();
            builder.Property(p => p.Type).HasMaxLength(4).HasColumnName("type");
            builder.Property(p => p.Currency).HasMaxLength(10).HasColumnName("currency");
            builder.Property(p => p.Price).HasColumnName("price");
            builder.Property<string>("exchange_id");
            builder.HasOne(p => p.Exchange).WithMany().HasForeignKey("exchange_id");
        }
    }
}
