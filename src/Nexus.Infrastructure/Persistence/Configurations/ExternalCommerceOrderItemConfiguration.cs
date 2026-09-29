using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities;

namespace Nexus.Infrastructure.Persistence.Configurations;

public sealed class ExternalCommerceOrderItemConfiguration : IEntityTypeConfiguration<ExternalCommerceOrderItem>
{
    public void Configure(EntityTypeBuilder<ExternalCommerceOrderItem> builder)
    {
        builder.ToTable("external_commerce_order_items");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Sku)
            .HasMaxLength(100);

        builder.Property(x => x.CategoryHint)
            .HasMaxLength(150);

        builder.Property(x => x.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.TotalPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Condition)
            .HasMaxLength(50);

        builder.HasOne(x => x.Order)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.ExternalCommerceOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.BankTransaction)
            .WithMany()
            .HasForeignKey(x => x.BankTransactionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.ExternalCommerceOrderId)
            .HasDatabaseName("ix_external_commerce_items_order_id");

        builder.HasIndex(x => x.BankTransactionId)
            .HasDatabaseName("ix_external_commerce_items_tx_id");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
