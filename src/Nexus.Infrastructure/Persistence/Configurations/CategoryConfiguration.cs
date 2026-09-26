using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;

namespace Nexus.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.NormalizedName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Icon)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("tag");

        builder.Property(x => x.ColorHex)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("#64748B");

        builder.Property(x => x.FlowType)
            .IsRequired();

        builder.Property(x => x.DefaultExpenseNature)
            .IsRequired();

        builder.Property(x => x.IsSystem)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasOne(x => x.ParentCategory)
            .WithMany(x => x.Subcategories)
            .HasForeignKey(x => x.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Categories)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices estratégicos
        builder.HasIndex(x => new { x.UserId, x.ParentCategoryId, x.NormalizedName })
            .HasDatabaseName("ix_categories_user_parent_normalized");

        builder.HasIndex(x => x.NormalizedName)
            .HasDatabaseName("ix_categories_normalized_name");

        builder.HasQueryFilter(x => !x.IsDeleted);

        // Seed data determinista
        builder.HasData(SystemCategoriesCatalog.AllCategories);
    }
}
