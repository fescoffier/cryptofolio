using Cryptofolio.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Provides entity configuration for <see cref="Asset"/>.
    /// </summary>
    public class AssetEntityConfiguration : IEntityTypeConfiguration<Asset>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Asset> builder)
        {
            builder.ToTable("asset");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasMaxLength(100).HasColumnName("id");
            builder.Property(p => p.Name).HasMaxLength(250).HasColumnName("name");
            builder.Property(p => p.Symbol).HasMaxLength(50).HasColumnName("symbol");
            builder.Property(p => p.Description).HasColumnType("text").HasColumnName("description");
        }
    }
}
