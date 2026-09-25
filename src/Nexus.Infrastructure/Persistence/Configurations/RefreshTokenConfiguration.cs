using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities;

namespace Nexus.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.TokenHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(rt => rt.TokenHash);

        builder.Property(rt => rt.ReplacedByTokenHash)
            .HasMaxLength(256);

        builder.Property(rt => rt.CreatedByIp)
            .HasMaxLength(45);

        builder.HasIndex(rt => rt.UserId);

        // Filtro global de borrado lógico
        builder.HasQueryFilter(rt => !rt.IsDeleted);
    }
}
