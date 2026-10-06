using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities;

namespace Nexus.Infrastructure.Persistence.Configurations;

public sealed class BankConnectionConfiguration : IEntityTypeConfiguration<BankConnection>
{
    public void Configure(EntityTypeBuilder<BankConnection> builder)
    {
        builder.ToTable("bank_connections");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InstitutionCode)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.InstitutionName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.LogoUrl)
            .HasMaxLength(500);

        builder.Property(x => x.ExternalSessionId)
            .HasMaxLength(255);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Accounts)
            .WithOne(x => x.BankConnection)
            .HasForeignKey(x => x.BankConnectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.Status })
            .HasDatabaseName("ix_bank_connections_user_id_status");

        builder.HasIndex(x => x.ExternalSessionId)
            .HasDatabaseName("ix_bank_connections_external_session_id");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
