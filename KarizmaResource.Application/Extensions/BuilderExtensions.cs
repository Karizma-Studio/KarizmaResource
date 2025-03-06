using KarizmaPlatform.Resources.Application.Processors;
using KarizmaPlatform.Resources.Application.Processors.Interfaces;
using KarizmaPlatform.Resources.Application.Services;
using KarizmaPlatform.Resources.Infrastructure;
using KarizmaPlatform.Resources.Infrastructure.Repositories;
using KarizmaPlatform.Resources.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace KarizmaPlatform.Resources.Application.Extensions;

public static class BuilderExtensions
{
    public static IServiceCollection AddKarizmaResource<TEnum, TDatabase>
        (this IServiceCollection services, List<ResourceStructure<TEnum>> resourceStructures = null!) where TEnum : struct, Enum where TDatabase : IResourceDatabase
    {
        services
            .AddScoped<IResourceRepository, ResourceRepository>()
            .AddScoped<IUserResourceRepository, UserResourceRepository>()
            .AddScoped<IResourceProcessor<TEnum>, ResourceProcessor<TEnum>>()
            .AddSingleton<ResourceEventManager>()
            .AddSingleton<ResourceCache<TEnum>>()
            .AddScoped<IResourceDatabase>(provider => provider.GetRequiredService<TDatabase>())
            .AddSingleton<ResourcesHostedService<TEnum>>()
            .AddHostedService(provider => provider.GetRequiredService<ResourcesHostedService<TEnum>>())
            .AddSingleton(resourceStructures);

        return services;
    }
}