using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Provides entity configuration for <see cref="Asset"/>.
    /// </summary>
    public class AssetEntityTypeConfiguration : IEntityTypeConfiguration<Asset>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Asset> builder)
        {
            builder.ToTable("asset");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasMaxLength(100).HasColumnName("id");
            builder.Property(p => p.Name).HasMaxLength(250).HasColumnName("name").IsRequired();
            builder.Property(p => p.Symbol).HasMaxLength(50).HasColumnName("symbol").IsRequired();
            builder.Property(p => p.Description).HasColumnType("text").HasColumnName("description");
            builder.Ignore(p => p.CurrentValue);
            builder.Property(p => p.ThumbImageUrl).HasColumnName("thumb_image_url");
            builder.Property(p => p.SmallImageUrl).HasColumnName("small_image_url");
            builder.Property(p => p.LargeImageUrl).HasColumnName("large_image_url");
            builder.HasIndex(p => p.Symbol);
        }
    }
}
