using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities;

namespace Nexus.Infrastructure.Persistence.Configurations;

public sealed class TransactionReconciliationConfiguration : IEntityTypeConfiguration<TransactionReconciliation>
{
    public void Configure(EntityTypeBuilder<TransactionReconciliation> builder)
    {
        builder.ToTable("transaction_reconciliations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MatchReason)
            .HasMaxLength(500);

        builder.Property(x => x.MatchedOrderItemIds)
            .HasColumnType("uuid[]");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.BankTransaction)
            .WithMany()
            .HasForeignKey(x => x.BankTransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ExternalCommerceOrder)
            .WithMany(x => x.Reconciliations)
            .HasForeignKey(x => x.ExternalCommerceOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.BankTransactionId, x.ExternalCommerceOrderId })
            .HasDatabaseName("ix_reconciliations_tx_order");

        builder.HasIndex(x => new { x.UserId, x.Status })
            .HasDatabaseName("ix_reconciliations_user_status");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
