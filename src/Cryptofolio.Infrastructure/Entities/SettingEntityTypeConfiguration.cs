using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Provides entity configuration for <see cref="Setting"/>.
    /// </summary>
    public class SettingEntityTypeConfiguration : IEntityTypeConfiguration<Setting>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Setting> builder)
        {
            builder.ToTable("setting");
            builder.HasKey(p => p.Key);
            builder.Property(p => p.Key).HasMaxLength(500).HasColumnName("key").IsRequired();
            builder.Property(p => p.Group).HasMaxLength(100).HasColumnName("group").IsRequired();
            builder.Property(p => p.Value).HasColumnType("text").HasColumnName("value").IsRequired();
            builder.HasIndex(p => p.Group);
        }
    }
}
