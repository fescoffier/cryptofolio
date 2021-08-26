using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Provides entity configuration for <see cref="TransferTransaction"/>.
    /// </summary>
    public class TransferTransactionEntityTypeConfiguration : IEntityTypeConfiguration<TransferTransaction>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<TransferTransaction> builder)
        {
            builder.HasBaseType<Transaction>();
            builder.Property(p => p.Source).HasMaxLength(100).HasColumnName("source").IsRequired();
            builder.Property(p => p.Destination).HasMaxLength(100).HasColumnName("destination").IsRequired();
            builder.HasIndex(p => p.Source);
            builder.HasIndex(p => p.Destination);
        }
    }
}
