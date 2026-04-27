using KarizmaPlatform.Resources.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KarizmaPlatform.Resources.Infrastructure.Configurations;

public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.HasIndex(r => r.Title)
            .IsUnique();

        builder.HasIndex(r => r.Type);

        builder.Property(b => b.CreatedDate)
            .HasDefaultValueSql("now()");

        builder.Property(b => b.UpdatedDate)
            .HasDefaultValueSql("now()");

        builder.Property(b => b.Type)
            .HasColumnType("resource_type");
    }
}
