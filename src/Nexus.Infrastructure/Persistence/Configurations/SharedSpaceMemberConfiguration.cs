using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities;

namespace Nexus.Infrastructure.Persistence.Configurations;

public sealed class SharedSpaceMemberConfiguration : IEntityTypeConfiguration<SharedSpaceMember>
{
    public void Configure(EntityTypeBuilder<SharedSpaceMember> builder)
    {
        builder.ToTable("shared_space_members");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DisplayName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Initials)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.AvatarColor)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Role)
            .IsRequired();

        builder.Property(x => x.DeclaredMonthlyIncome)
            .HasPrecision(18, 2);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.SharedSpaceId, x.UserId })
            .HasDatabaseName("ix_shared_space_members_space_user");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
