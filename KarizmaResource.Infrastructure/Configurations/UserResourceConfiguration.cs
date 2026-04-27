using KarizmaPlatform.Resources.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KarizmaPlatform.Resources.Infrastructure.Configurations;

public class UserResourceConfiguration : IEntityTypeConfiguration<UserResource>
{
    public void Configure(EntityTypeBuilder<UserResource> builder)
    {
        builder.HasIndex(x => new { x.UserId })
            .HasFilter("deleted_at IS NULL");

        builder.HasIndex(x => new { x.UserId, x.ResourceId })
            .HasFilter("deleted_at IS NULL");

        builder.HasIndex(x => new { x.UserId, x.ResourceId, x.ExpireDate })
            .HasFilter("deleted_at IS NULL");

        builder.HasIndex(x => new { x.UserId, x.ResourceId, x.CollectableId })
            .HasFilter("(collectable_id IS NOT NULL) AND (deleted_at IS NULL)");

        builder.HasIndex(x => x.ResourceId);

        builder.Property(b => b.CreatedDate)
            .HasDefaultValueSql("now()");

        builder.Property(b => b.UpdatedDate)
            .HasDefaultValueSql("now()");
    }
}
