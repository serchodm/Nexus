using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities;

namespace Nexus.Infrastructure.Persistence.Configurations;

public sealed class TransactionSplitItemConfiguration : IEntityTypeConfiguration<TransactionSplitItem>
{
    public void Configure(EntityTypeBuilder<TransactionSplitItem> builder)
    {
        builder.ToTable("transaction_split_items");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(255);

        builder.Property(x => x.ExpenseNature)
            .IsRequired();

        builder.HasOne(x => x.BankTransaction)
            .WithMany(x => x.SplitItems)
            .HasForeignKey(x => x.BankTransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.SplitItems)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Subcategory)
            .WithMany()
            .HasForeignKey(x => x.SubcategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.BankTransactionId)
            .HasDatabaseName("ix_transaction_split_items_transaction_id");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
