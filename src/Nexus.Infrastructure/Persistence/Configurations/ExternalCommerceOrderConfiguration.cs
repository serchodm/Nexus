using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities;

namespace Nexus.Infrastructure.Persistence.Configurations;

public sealed class ExternalCommerceOrderConfiguration : IEntityTypeConfiguration<ExternalCommerceOrder>
{
    public void Configure(EntityTypeBuilder<ExternalCommerceOrder> builder)
    {
        builder.ToTable("external_commerce_orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ExternalOrderId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(3)
            .HasDefaultValue("EUR")
            .IsRequired();

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.ShippingFee)
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);

        builder.Property(x => x.TaxAmount)
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);

        builder.Property(x => x.RefundAmount)
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);

        builder.Property(x => x.CounterpartyName)
            .HasMaxLength(255);

        builder.Property(x => x.RawDataJson)
            .HasColumnType("jsonb");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.Provider, x.ExternalOrderId })
            .IsUnique()
            .HasDatabaseName("ix_external_commerce_orders_unique");

        builder.HasIndex(x => new { x.UserId, x.OrderDate })
            .HasDatabaseName("ix_external_commerce_orders_user_date");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
