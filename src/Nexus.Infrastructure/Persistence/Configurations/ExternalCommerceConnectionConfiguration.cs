using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities;

namespace Nexus.Infrastructure.Persistence.Configurations;

public sealed class ExternalCommerceConnectionConfiguration : IEntityTypeConfiguration<ExternalCommerceConnection>
{
    public void Configure(EntityTypeBuilder<ExternalCommerceConnection> builder)
    {
        builder.ToTable("external_commerce_connections");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AccountEmail)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.EncryptedAccessToken)
            .IsRequired();

        builder.Property(x => x.LastSyncStatus)
            .HasMaxLength(255);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.Provider })
            .IsUnique()
            .HasDatabaseName("ix_commerce_connections_user_provider");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
