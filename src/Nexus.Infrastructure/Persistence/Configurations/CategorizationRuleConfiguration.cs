using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities;

namespace Nexus.Infrastructure.Persistence.Configurations;

public sealed class CategorizationRuleConfiguration : IEntityTypeConfiguration<CategorizationRule>
{
    public void Configure(EntityTypeBuilder<CategorizationRule> builder)
    {
        builder.ToTable("categorization_rules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Pattern)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.NormalizedPattern)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.MatchType)
            .IsRequired();

        builder.Property(x => x.Priority)
            .IsRequired()
            .HasDefaultValue(100);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(x => x.User)
            .WithMany(x => x.CategorizationRules)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.TargetCategory)
            .WithMany()
            .HasForeignKey(x => x.TargetCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TargetSubcategory)
            .WithMany()
            .HasForeignKey(x => x.TargetSubcategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices estratégicos
        builder.HasIndex(x => new { x.UserId, x.IsActive, x.Priority })
            .HasDatabaseName("ix_categorization_rules_user_active_priority");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
