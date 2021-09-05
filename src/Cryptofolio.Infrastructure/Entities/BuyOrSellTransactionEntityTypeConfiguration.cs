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
            builder.Property(p => p.Price).HasColumnName("price").IsRequired();
            builder.Property<string>("currency_id");
            builder.HasOne(p => p.Currency).WithMany().HasForeignKey("currency_id").IsRequired();
            builder.Property(p => p.InitialValue).HasColumnName("initial_value").IsRequired();
            builder.Property(p => p.Change).HasColumnName("change").IsRequired();
            builder.HasIndex(p => p.Type);
        }
    }
}
