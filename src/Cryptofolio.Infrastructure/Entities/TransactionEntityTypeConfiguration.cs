using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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
            builder.Property(p => p.Date).HasColumnName("date").HasConversion(p => p, p => p.ToUniversalTime());
            builder.Property(p => p.Qty).HasColumnName("qty");
            builder.Property(p => p.InitialValue).HasColumnName("initial_value").IsRequired();
            builder.Property(p => p.CurrentValue).HasColumnName("current_value").IsRequired();
            builder.Property(p => p.Change).HasColumnName("change").IsRequired();
            builder.Property(p => p.Note).HasColumnType("text").HasColumnName("note");
            builder.Property<string>("wallet_id");
            builder.HasOne(p => p.Wallet).WithMany().HasForeignKey("wallet_id").IsRequired();
            builder.Property<string>("asset_id");
            builder.HasOne(p => p.Asset).WithMany().HasForeignKey("asset_id").IsRequired();
            builder.Property<string>("exchange_id");
            builder.HasOne(p => p.Exchange).WithMany().HasForeignKey("exchange_id");
            builder.HasIndex(p => p.Date).HasSortOrder(SortOrder.Descending);
        }
    }
}
