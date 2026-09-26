using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities;

namespace Nexus.Infrastructure.Persistence.Configurations;

public sealed class StatementImportConfiguration : IEntityTypeConfiguration<StatementImport>
{
    public void Configure(EntityTypeBuilder<StatementImport> builder)
    {
        builder.ToTable("statement_imports");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.FileHashSha256)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.StartingBalance)
            .HasPrecision(18, 2);

        builder.Property(x => x.EndingBalance)
            .HasPrecision(18, 2);

        builder.HasOne(x => x.BankAccount)
            .WithMany(x => x.Imports)
            .HasForeignKey(x => x.BankAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índice Único condicional: Permite re-importar tras rollback (soft-delete)
        builder.HasIndex(x => new { x.BankAccountId, x.FileHashSha256 })
            .IsUnique()
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ix_statement_imports_account_hash_unique");

        builder.HasIndex(x => x.BankAccountId)
            .HasDatabaseName("ix_statement_imports_bank_account_id");

        // Query filter global para soft-delete
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
