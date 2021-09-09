using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Provides entity configuration for <see cref="Exchange"/>.
    /// </summary>
    public class ExchangeEntityTypeConfiguration : IEntityTypeConfiguration<Exchange>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Exchange> builder)
        {
            builder.ToTable("exchange");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasMaxLength(100).HasColumnName("id");
            builder.Property(p => p.Name).HasMaxLength(250).HasColumnName("name").IsRequired();
            builder.Property(p => p.Description).HasColumnType("text").HasColumnName("description");
            builder.Property(p => p.YearEstablished).HasColumnName("year_established");
            builder.Property(p => p.Url).HasMaxLength(2048).HasColumnName("url").IsRequired();
            builder.Property(p => p.Image).HasMaxLength(2048).HasColumnName("image").IsRequired();
        }
    }
}
