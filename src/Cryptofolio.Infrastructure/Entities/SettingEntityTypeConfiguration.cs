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
            builder.ToTable("settings");
            builder.HasKey(p => p.Key);
            builder.Property(p => p.Key).HasColumnType("varchar(max)").HasColumnName("key");
            builder.Property(p => p.Group).HasMaxLength(100).HasColumnName("group");
            builder.Property(p => p.Value).HasColumnType("text").HasColumnName("value");
        }
    }
}
