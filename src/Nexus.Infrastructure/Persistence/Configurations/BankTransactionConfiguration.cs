using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities;

namespace Nexus.Infrastructure.Persistence.Configurations;

public sealed class BankTransactionConfiguration : IEntityTypeConfiguration<BankTransaction>
{
    public void Configure(EntityTypeBuilder<BankTransaction> builder)
    {
        builder.ToTable("bank_transactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("EUR");

        builder.Property(x => x.BalanceAfter)
            .HasPrecision(18, 2);

        builder.Property(x => x.BankCategory)
            .HasMaxLength(100);

        builder.Property(x => x.BankSubcategory)
            .HasMaxLength(100);

        builder.Property(x => x.BankMovementType)
            .HasMaxLength(100);

        builder.Property(x => x.RawDataJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.Fingerprint)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasOne(x => x.BankAccount)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.BankAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StatementImport)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.StatementImportId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices Estratégicos de Alto Rendimiento
        builder.HasIndex(x => new { x.BankAccountId, x.Fingerprint })
            .IsUnique()
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ix_bank_transactions_account_fingerprint_unique");

        builder.HasIndex(x => x.StatementImportId)
            .HasDatabaseName("ix_bank_transactions_statement_import_id");

        builder.HasIndex(x => new { x.BankAccountId, x.OperationDate })
            .HasDatabaseName("ix_bank_transactions_account_date");

        // Query filter global para soft-delete
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
