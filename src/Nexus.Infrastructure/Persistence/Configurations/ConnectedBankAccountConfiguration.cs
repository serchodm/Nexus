using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities;

namespace Nexus.Infrastructure.Persistence.Configurations;

public sealed class ConnectedBankAccountConfiguration : IEntityTypeConfiguration<ConnectedBankAccount>
{
    public void Configure(EntityTypeBuilder<ConnectedBankAccount> builder)
    {
        builder.ToTable("connected_bank_accounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ExternalAccountId)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Iban)
            .HasMaxLength(34);

        builder.Property(x => x.AccountName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.CurrentBalance)
            .HasPrecision(18, 2);

        builder.Property(x => x.AvailableBalance)
            .HasPrecision(18, 2);

        builder.HasOne(x => x.BankConnection)
            .WithMany(x => x.Accounts)
            .HasForeignKey(x => x.BankConnectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.InternalBankAccount)
            .WithMany()
            .HasForeignKey(x => x.InternalBankAccountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.BankConnectionId)
            .HasDatabaseName("ix_connected_bank_accounts_bank_connection_id");

        builder.HasIndex(x => x.ExternalAccountId)
            .HasDatabaseName("ix_connected_bank_accounts_external_account_id");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
