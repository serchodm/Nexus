using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities;

namespace Nexus.Infrastructure.Persistence.Configurations;

public sealed class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.ToTable("bank_accounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.BankName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.BankCode)
            .HasMaxLength(4);

        builder.Property(x => x.Iban)
            .HasMaxLength(34);

        builder.Property(x => x.Last4Digits)
            .IsRequired()
            .HasMaxLength(4);

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("EUR");

        builder.Property(x => x.OwnershipPercentage)
            .HasPrecision(5, 2)
            .HasDefaultValue(100.00m);

        builder.Property(x => x.CurrentBalance)
            .HasPrecision(18, 2);

        builder.Property(x => x.LinkedCardsLast4)
            .HasColumnType("text[]");

        // Clave foránea lógica hacia Users
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("ix_bank_accounts_user_id");

        // Índice condicional: evita duplicar IBAN activo por usuario pero permite soft-delete
        builder.HasIndex(x => new { x.UserId, x.Iban })
            .IsUnique()
            .HasFilter("is_deleted = false AND iban IS NOT NULL")
            .HasDatabaseName("ix_bank_accounts_user_iban_unique");

        // Query filter global para soft-delete
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
