using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities;

namespace Nexus.Infrastructure.Persistence.Configurations;

public sealed class SharedExpenseEntryConfiguration : IEntityTypeConfiguration<SharedExpenseEntry>
{
    public void Configure(EntityTypeBuilder<SharedExpenseEntry> builder)
    {
        builder.ToTable("shared_expense_entries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.PaidByShare)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.OwedShare)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.CategoryName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.CategoryIcon)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.HasOne(x => x.PaidByMember)
            .WithMany()
            .HasForeignKey(x => x.PaidByMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BankTransaction)
            .WithMany()
            .HasForeignKey(x => x.BankTransactionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.TransactionSplitItem)
            .WithMany()
            .HasForeignKey(x => x.TransactionSplitItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Settlement)
            .WithMany(x => x.SettledExpenses)
            .HasForeignKey(x => x.SettlementId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.SharedSpaceId, x.Status })
            .HasDatabaseName("ix_shared_expense_entries_space_status");

        builder.HasIndex(x => x.BankTransactionId)
            .HasDatabaseName("ix_shared_expense_entries_bank_tx_id");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
