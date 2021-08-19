using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Provides entity configuration for <see cref="Transaction"/>.
    /// </summary>
    public class TransactionEntityTypeConfiguration : IEntityTypeConfiguration<Transaction>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("transaction");
            builder.HasDiscriminator<string>("discriminator");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasMaxLength(36).HasColumnName("id");
            builder.Property(p => p.Date).HasColumnName("date");
            builder.Property(p => p.Qty).HasColumnName("qty");
            builder.Property(p => p.Note).HasColumnType("text").HasColumnName("note");
            builder.Property<string>("wallet_id");
            builder.HasOne(p => p.Wallet).WithMany().HasForeignKey("wallet_id");
            builder.Property<string>("asset_id");
            builder.HasOne(p => p.Asset).WithMany().HasForeignKey("asset_id");
        }
    }
}
