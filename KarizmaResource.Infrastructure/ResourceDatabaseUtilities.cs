using KarizmaPlatform.Resources.Domain.Models;
using KarizmaPlatform.Resources.SharedClasses.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace KarizmaPlatform.Resources.Infrastructure;

public class ResourceDatabaseUtilities
{
    public static void ConfigureDatabase<T>(ModelBuilder modelBuilder) where T : class, IResourceUser
    {
        modelBuilder.HasPostgresEnum<ResourceType>();
        
        modelBuilder.Entity<UserResource>()
            .HasOne<T>()
            .WithMany()
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserResource>().Ignore(ur => ur.User);

        modelBuilder.Entity<Resource>()
            .Property(r => r.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<Resource>()
            .HasIndex(r => r.Title)
            .IsUnique();
        
        
        modelBuilder.Entity<UserResource>()
            .HasIndex(x => new { x.UserId })
            .HasFilter("deleted_at IS NULL");

        modelBuilder.Entity<UserResource>()
            .HasIndex(x => new { x.UserId, x.ResourceId })
            .HasFilter("deleted_at IS NULL");

        modelBuilder.Entity<UserResource>()
            .HasIndex(x => new { x.UserId, x.ResourceId, x.CollectableId })
            .HasFilter("(collectable_id IS NOT NULL) AND (deleted_at IS NULL)");

        modelBuilder.Entity<Resource>()
            .Property(b => b.CreatedDate)
            .HasDefaultValueSql("now()");

        modelBuilder.Entity<Resource>()
            .Property(b => b.UpdatedDate)
            .HasDefaultValueSql("now()");
        
        modelBuilder.Entity<Resource>()
            .Property(b => b.Type)
            .HasColumnType("resource_type");

        modelBuilder.Entity<UserResource>()
            .Property(b => b.CreatedDate)
            .HasDefaultValueSql("now()");

        modelBuilder.Entity<UserResource>()
            .Property(b => b.UpdatedDate)
            .HasDefaultValueSql("now()");
    }
    
    public static void MapEnums(NpgsqlDataSourceBuilder dataSourceBuilder)
    {
        dataSourceBuilder.MapEnum<ResourceType>("resource_type");
    }
}