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
            builder.Property(p => p.Type).HasMaxLength(4).HasColumnName("type").IsRequired();
            builder.Property(p => p.Currency).HasMaxLength(10).HasColumnName("currency").IsRequired();
            builder.Property(p => p.Price).HasColumnName("price");
        }
    }
}
