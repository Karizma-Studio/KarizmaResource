using KarizmaPlatform.Resources.Domain.Models;
using KarizmaPlatform.Resources.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KarizmaPlatform.Resources.Application.Services;

public class ResourcesHostedService<TEnum>(
    IServiceScopeFactory scopeFactory,
    ResourceCache<TEnum> resourceCache,
    List<ResourceStructure<TEnum>> resourceStructures) : IHostedService where TEnum : struct, Enum
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await WriteResourcesToDatabase();
        await FillResourceCache();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Pending Resource Timer Stopping.");
        return Task.CompletedTask;
    }

    private async Task FillResourceCache()
    {
        using var scope = scopeFactory.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<IResourceDatabase>();
        var resources = await database.GetResources().ToListAsync();
        var resourcesMap = new Dictionary<TEnum, Resource>();

        foreach (var resource in resources)
            if (Enum.TryParse<TEnum>(resource.Title, out var label))
                resourcesMap.Add(label, resource);
            else
                Console.WriteLine($"resource {resource.Title} is not valid");

        resourceCache.Populate(resourcesMap);
    }

    private async Task WriteResourcesToDatabase()
    {
        using var scope = scopeFactory.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<IResourceDatabase>();
        var resources = await database.GetResources().ToListAsync();

        foreach (var structure in resourceStructures.Where(structure => !resources.Any(r => r.Title.Equals(structure.Title.ToString()))))
            database.GetResources().Add(new Resource
            {
                Id = structure.Id,
                Title = structure.Title.ToString(),
                Type = structure.Type,
                Category = structure.Category
            });

        await database.SaveChangesAsync();
    }
}