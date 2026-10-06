using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities;

namespace Nexus.Infrastructure.Persistence.Configurations;

public sealed class SharedSettlementConfiguration : IEntityTypeConfiguration<SharedSettlement>
{
    public void Configure(EntityTypeBuilder<SharedSettlement> builder)
    {
        builder.ToTable("shared_settlements");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Method)
            .IsRequired();

        builder.Property(x => x.ReferenceCode)
            .HasMaxLength(100);

        builder.Property(x => x.SettledAt)
            .IsRequired();

        builder.HasOne(x => x.PayerMember)
            .WithMany()
            .HasForeignKey(x => x.PayerMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReceiverMember)
            .WithMany()
            .HasForeignKey(x => x.ReceiverMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LinkedTransaction)
            .WithMany()
            .HasForeignKey(x => x.LinkedTransactionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.SharedSpaceId)
            .HasDatabaseName("ix_shared_settlements_space_id");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
