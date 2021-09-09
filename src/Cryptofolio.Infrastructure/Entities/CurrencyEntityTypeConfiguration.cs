using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Provides entity configuration for <see cref="Currency"/>.
    /// </summary>
    public class CurrencyEntityTypeConfiguration : IEntityTypeConfiguration<Currency>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Currency> builder)
        {
            builder.ToTable("currency");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasMaxLength(36).HasColumnName("id");
            builder.Property(p => p.Name).HasMaxLength(100).HasColumnName("name").IsRequired();
            builder.Property(p => p.Code).HasMaxLength(3).HasColumnName("code").IsRequired();
            builder.Property(p => p.Symbol).HasMaxLength(3).HasColumnName("symbol").IsRequired();
            builder.Property(p => p.Precision).HasDefaultValue(2).HasColumnName("precision").IsRequired();
            builder.HasIndex(p => p.Code).IsUnique();
        }
    }
}
