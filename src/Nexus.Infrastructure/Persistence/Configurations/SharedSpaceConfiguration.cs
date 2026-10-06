using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities;

namespace Nexus.Infrastructure.Persistence.Configurations;

public sealed class SharedSpaceConfiguration : IEntityTypeConfiguration<SharedSpace>
{
    public void Configure(EntityTypeBuilder<SharedSpace> builder)
    {
        builder.ToTable("shared_spaces");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.DefaultSplitRule)
            .IsRequired();

        builder.Property(x => x.DefaultOwnerPercentage)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(x => x.DefaultMemberPercentage)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.HasMany(x => x.Members)
            .WithOne(x => x.SharedSpace)
            .HasForeignKey(x => x.SharedSpaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Expenses)
            .WithOne(x => x.SharedSpace)
            .HasForeignKey(x => x.SharedSpaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Settlements)
            .WithOne(x => x.SharedSpace)
            .HasForeignKey(x => x.SharedSpaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
