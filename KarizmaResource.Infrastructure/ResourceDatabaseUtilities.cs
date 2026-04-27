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

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ResourceDatabaseUtilities).Assembly);
        
        modelBuilder.Entity<UserResource>()
            .HasOne<T>()
            .WithMany()
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserResource>().Ignore(ur => ur.User);
    }
    
    public static void MapEnums(NpgsqlDataSourceBuilder dataSourceBuilder)
    {
        dataSourceBuilder.MapEnum<ResourceType>("resource_type");
    }
}