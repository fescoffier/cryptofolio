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
            builder.Property(p => p.Name).HasMaxLength(250).HasColumnName("name").IsRequired();
            builder.Property(p => p.Description).HasColumnType("text").HasColumnName("description");
            builder.Property<string>("currency_id");
            builder.HasOne(p => p.Currency).WithMany().HasForeignKey("currency_id").IsRequired();
            builder.Property(p => p.CurrentValue).HasColumnName("current_value").IsRequired();
            builder.Property(p => p.Change).HasColumnName("change").IsRequired();
            builder.Property(p => p.Selected).HasDefaultValue(false).HasColumnName("selected").IsRequired();
            builder.Property(p => p.UserId).HasMaxLength(36).HasColumnName("user_id").IsRequired();
        }
    }
}
