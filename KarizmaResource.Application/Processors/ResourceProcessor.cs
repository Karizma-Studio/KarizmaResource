using System.Text.Json;
using KarizmaPlatform.Resources.Application.Processors.Interfaces;
using KarizmaPlatform.Resources.Application.Services;
using KarizmaPlatform.Resources.Domain.Models;
using KarizmaPlatform.Resources.Infrastructure.Repositories.Interfaces;
using KarizmaPlatform.Resources.SharedClasses.Enums;
using KarizmaPlatform.Resources.SharedClasses.JsonSchemas;

namespace KarizmaPlatform.Resources.Application.Processors;

public class ResourceProcessor<T>(
    IUserResourceRepository userResourceRepository,
    ResourceCache<T> resourceCache) : IResourceProcessor<T> where T : struct, Enum
{
    public event EventHandler<ResourceChangedEventArgs> ResourceChanged;
    public Resource GetResource(T resourceLabel)
    {
        return resourceCache.GetResource(resourceLabel);
    }
    
    protected virtual void OnResourceChanged(ResourceChangedEventArgs e)
    {
        ResourceChanged?.Invoke(this, e);
    }

    public async Task<bool> CanChange(long userId, ResourceChange change)
    {
        var resource = resourceCache.GetResource(change.GetResourceEnum<T>());
        var userResources = await userResourceRepository.FindUserResource(userId, resource.Id, false);

        return CanChange(userResources, change);
    }

    public async Task<bool> AddTransaction(long userId, List<ResourceChange> resourceChanges)
    {
        foreach (var resourceChange in resourceChanges)
        {
            var result = await AddTransaction(userId, resourceChange);
            if (!result)
                return false;
        }

        return true;
    }

    private async Task<bool> AddTransaction(long userId, ResourceChange change)
    {
        try
        {
            var resource = resourceCache.GetResource(change.GetResourceEnum<T>());
            if(resource.Type == ResourceType.Custom)
            {
                OnResourceChanged(new ResourceChangedEventArgs { UserId = userId, ResourceChange = change });
                return true;
            }
                
            ArgumentNullException.ThrowIfNull(resource, "resource != null");

            if (ResourceType.Collectable.Equals(resource.Type))
            {
                ArgumentNullException.ThrowIfNull(change.CollectableId, "resourceChange.CollectableId != null");
                var userCollectable = await userResourceRepository.FindUserCollectable(userId, resource.Id, change.CollectableId.Value);

                if (userCollectable is null)
                    await CreateNewUserResource(userId, change);
                else
                    await UpdateUserResource(new List<UserResource> { userCollectable }, change);
            }
            else
            {
                var userResources = await userResourceRepository.FindUserResource(userId, resource.Id);

                if (userResources.Count == 0 && change.Amount < 0)
                    return false;

                if (userResources.Count == 0 || (ResourceType.ExpiringNumeric.Equals(resource.Type) && change.Amount > 0))
                    await CreateNewUserResource(userId, change);
                else
                    await UpdateUserResource(userResources, change);
            }

            OnResourceChanged(new ResourceChangedEventArgs { UserId = userId, ResourceChange = change });
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"AddTransaction Error, userId: {userId}, change: {JsonSerializer.Serialize(change)} --- {e.StackTrace}");
            OnResourceChanged(new ResourceChangedEventArgs { UserId = userId, ResourceChange = change });
            return false;
        }
    }

    #region Create User Resource

    private readonly Dictionary<ResourceType, Func<long, Resource, ResourceChange, UserResource>> resourceCreators = new()
    {
        { ResourceType.Numeric, CreateNumeric },
        { ResourceType.Timely, CreateTimely },
        { ResourceType.Mixed, CreateMixed },
        { ResourceType.ExpiringNumeric, CreateExpiringNumeric },
        { ResourceType.Collectable, CreateCollectable }
    };

    private async Task CreateNewUserResource(long userId, ResourceChange change)
    {
        if (change.Amount < 0 || change.Duration < 0) throw new ArgumentException("Create negative values not available");

        var resource = resourceCache.GetResource(change.GetResourceEnum<T>());
        ArgumentNullException.ThrowIfNull(resource, "Resource object cant be null");

        if (resourceCreators.TryGetValue(resource.Type, out var creator))
            await userResourceRepository.Add(creator(userId, resource, change));
        else
            throw new ArgumentException($"Resource creator for type '{resource.Type}' not found.");
    }

    private static UserResource CreateNumeric(long userId, Resource resource, ResourceChange change)
    {
        if (!ResourceType.Numeric.Equals(resource.Type))
            throw new ArgumentException("Using numeric function on non-numeric resource");
        ArgumentNullException.ThrowIfNull(change.Amount, "Amount");

        return new UserResource
        {
            UserId = userId,
            ResourceId = resource.Id,
            Amount = change.Amount,
            ExpireDate = null,
            CollectableId = null
        };
    }

    private static UserResource CreateTimely(long userId, Resource resource, ResourceChange change)
    {
        if (!ResourceType.Timely.Equals(resource.Type))
            throw new ArgumentException("Using timely function on non-timely resource");
        ArgumentNullException.ThrowIfNull(change.Duration, "Duration");

        return new UserResource
        {
            UserId = userId,
            ResourceId = resource.Id,
            Amount = null,
            ExpireDate = DateTimeOffset.UtcNow.AddSeconds(change.Duration.Value),
            CollectableId = null
        };
    }

    private static UserResource CreateMixed(long userId, Resource resource, ResourceChange change)
    {
        if (!ResourceType.Mixed.Equals(resource.Type))
            throw new ArgumentException("Using mixed function on non-mixed resource");

        return new UserResource
        {
            UserId = userId,
            ResourceId = resource.Id,
            Amount = change.Amount ?? 0,
            ExpireDate = DateTimeOffset.UtcNow.AddSeconds(change.Duration ?? 0),
            CollectableId = null
        };
    }

    private static UserResource CreateExpiringNumeric(long userId, Resource resource, ResourceChange change)
    {
        if (!ResourceType.ExpiringNumeric.Equals(resource.Type))
            throw new ArgumentException("Using expiring-numeric function on non-expiring-numeric resource");

        ArgumentNullException.ThrowIfNull(change.Amount, "Amount");
        ArgumentNullException.ThrowIfNull(change.Duration, "Duration");

        return new UserResource
        {
            UserId = userId,
            ResourceId = resource.Id,
            Amount = change.Amount,
            ExpireDate = DateTimeOffset.UtcNow.AddSeconds(change.Duration.Value),
            CollectableId = null
        };
    }

    private static UserResource CreateCollectable(long userId, Resource resource, ResourceChange change)
    {
        if (!ResourceType.Collectable.Equals(resource.Type))
            throw new ArgumentException("Using collectable function on non-collectable resource");

        ArgumentNullException.ThrowIfNull(change.Amount, "Amount");
        ArgumentNullException.ThrowIfNull(change.CollectableId, "CollectableId");

        return new UserResource
        {
            UserId = userId,
            ResourceId = resource.Id,
            Amount = change.Amount,
            ExpireDate = null,
            CollectableId = change.CollectableId
        };
    }

    #endregion

    #region Update User Resource

    private readonly Dictionary<ResourceType, Func<List<UserResource>, Resource, ResourceChange, List<UserResource>>> resourceUpdaters = new()
    {
        { ResourceType.Numeric, UpdateNumeric },
        { ResourceType.Timely, UpdateTimely },
        { ResourceType.Mixed, UpdateMixed },
        { ResourceType.ExpiringNumeric, UpdateExpiringNumeric },
        { ResourceType.Collectable, UpdateCollectable }
    };

    private async Task UpdateUserResource(List<UserResource> userResources, ResourceChange change)
    {
        if (!CanChange(userResources, change))
            throw new ArgumentException("Cant Change Resource With this resource change");

        var resource = resourceCache.GetResource(change.GetResourceEnum<T>());
        ArgumentNullException.ThrowIfNull(resource, "Resource object cant be null");

        if (resourceUpdaters.TryGetValue(resource.Type, out var updater))
        {
            var updatedResources = updater(userResources, resource, change);
            foreach (var updatedResource in updatedResources)
                if (updatedResource.DeletedDate is null)
                    await userResourceRepository.Update(updatedResource);
                else
                    await userResourceRepository.DeleteById(updatedResource.Id);

            OnResourceChanged(new ResourceChangedEventArgs { UserId = userResources[0].UserId, ResourceChange = change });
        }
        else
        {
            throw new ArgumentException($"Resource updater for type '{resource.Type}' not found.");
        }
    }

    private static List<UserResource> UpdateNumeric(List<UserResource> userResources, Resource resource, ResourceChange change)
    {
        if (!ResourceType.Numeric.Equals(resource.Type))
            throw new ArgumentException("Using numeric function on non-numeric resource");

        if (userResources.Count > 1)
            throw new ArgumentException($"Multiple numeric user resources, userId: {userResources[0].UserId}");

        var userResource = userResources[0];
        userResource.Amount += change.Amount;

        return [userResource];
    }

    private static List<UserResource> UpdateTimely(List<UserResource> userResources, Resource resource, ResourceChange change)
    {
        if (!ResourceType.Timely.Equals(resource.Type))
            throw new ArgumentException("Using timely function on non-timely resource");

        if (userResources.Count > 1)
            throw new ArgumentException($"Multiple timely user resources, userId: {userResources[0].UserId}");

        var userResource = userResources[0];
        userResource.ExpireDate = userResource.IsExpired ? DateTimeOffset.UtcNow.AddSeconds(change.Duration!.Value) : userResource.ExpireDate!.Value.AddSeconds(change.Duration!.Value);

        return [userResource];
    }

    private static List<UserResource> UpdateMixed(List<UserResource> userResources, Resource resource, ResourceChange change)
    {
        if (!ResourceType.Mixed.Equals(resource.Type))
            throw new ArgumentException("Using mixed function on non-mixed resource");

        if (userResources.Count > 1)
            throw new ArgumentException($"Multiple mixed user resources, userId: {userResources[0].UserId}");

        var userResource = userResources[0];

        if (change.Amount > 0 || userResource.IsExpired)
            userResource.Amount += change.Amount ?? 0;

        userResource.ExpireDate = userResource.IsExpired ? DateTimeOffset.UtcNow.AddSeconds(change.Duration ?? 0) : userResource.ExpireDate!.Value.AddSeconds(change.Duration ?? 0);

        return [userResource];
    }

    private static List<UserResource> UpdateCollectable(List<UserResource> userResources, Resource resource, ResourceChange change)
    {
        if (!ResourceType.Collectable.Equals(resource.Type))
            throw new ArgumentException("Using collectable function on non-collectable resource");

        if (userResources.Count > 1)
            throw new ArgumentException($"Multiple mixed user resources, userId: {userResources[0].UserId}");

        var userResource = userResources[0];
        userResource.Amount += change.Amount;

        return [userResource];
    }

    private static List<UserResource> UpdateExpiringNumeric(List<UserResource> userResources, Resource resource, ResourceChange change)
    {
        if (!ResourceType.ExpiringNumeric.Equals(resource.Type))
            throw new ArgumentException("Using expiring numeric function on non-expiring-numeric resource");

        foreach (var userResource in userResources)
            if (userResource.IsExpired)
            {
                userResource.DeletedDate = DateTimeOffset.UtcNow;
            }
            else
            {
                if (Math.Abs(change.Amount!.Value) > userResource.Amount)
                {
                    change.Amount += userResource.Amount;
                    userResource.Amount = 0;
                    userResource.DeletedDate = DateTimeOffset.UtcNow;
                }
                else
                {
                    userResource.Amount += change.Amount;
                    break;
                }
            }

        return userResources;
    }

    #endregion

    #region Change Check

    private readonly Dictionary<ResourceType, Func<List<UserResource>, Resource, ResourceChange, bool>> resourceCheckers = new()
    {
        { ResourceType.Numeric, CheckNumeric },
        { ResourceType.Timely, CheckTimely },
        { ResourceType.Mixed, CheckMixed },
        { ResourceType.ExpiringNumeric, CheckExpiringNumeric },
        { ResourceType.Collectable, CheckCollectable }
    };

    private bool CanChange(List<UserResource> userResources, ResourceChange change)
    {
        var resource = resourceCache.GetResource(change.GetResourceEnum<T>());
        ArgumentNullException.ThrowIfNull(resource, "resource != null");

        if (!ResourceType.ExpiringNumeric.Equals(resource.Type) && userResources.Count > 1)
            throw new ArgumentException($"Multiple Non-Expiring User Resources, userId: {userResources[0].UserId}");

        if (resourceCheckers.TryGetValue(resource.Type, out var checker)) return checker(userResources, resource, change);

        throw new ArgumentException($"Resource checker for type '{resource.Type}' not found.");
    }

    private static bool CheckNumeric(List<UserResource> userResources, Resource resource, ResourceChange change)
    {
        if (!ResourceType.Numeric.Equals(resource.Type))
            throw new ArgumentException("Using numeric function on non-numeric resource");

        var userResource = userResources.Count == 0 ? null : userResources[0];

        ArgumentNullException.ThrowIfNull(change.Amount, "resourceChange.Amount != null");
        if (change.Amount >= 0)
            return true;
        if (userResource is null)
            return false;
        return change.Amount + userResource.Amount >= 0;
    }

    private static bool CheckTimely(List<UserResource> userResources, Resource resource, ResourceChange change)
    {
        if (!ResourceType.Timely.Equals(resource.Type))
            throw new ArgumentException("Using timely function on non-timely resource");

        var userResource = userResources.Count == 0 ? null : userResources[0];

        ArgumentNullException.ThrowIfNull(change.Duration, "change.Duration != null");
        if (change.Duration >= 0)
            return true;
        if (userResource is null)
            return false;
        ArgumentNullException.ThrowIfNull(userResource.ExpireDate, "userResource.ExpireDate != null");
        if (userResource.IsExpired)
            return false;
        return userResource.ExpireDate.Value.AddSeconds(change.Duration.Value) >= DateTimeOffset.UtcNow;
    }

    private static bool CheckMixed(List<UserResource> userResources, Resource resource, ResourceChange change)
    {
        if (!ResourceType.Mixed.Equals(resource.Type))
            throw new ArgumentException("Using mixed function on non-mixed resource");

        var userResource = userResources.Count == 0 ? null : userResources[0];

        if (change.Duration < 0)
            throw new ArgumentException("Mixed Resource Duration Change Cant be Negative");

        if (change is { Amount: >= 0, Duration: >= 0 } or
            { Amount: >= 0, Duration: null } or { Amount: null, Duration: >= 0 })
            return true;
        if (userResource is null)
            return false;
        if (!userResource.IsExpired)
            return true;

        return (userResource.Amount + change.Amount ?? 0) >= 0;
    }

    private static bool CheckCollectable(List<UserResource> userResources, Resource resource, ResourceChange change)
    {
        if (!ResourceType.Collectable.Equals(resource.Type))
            throw new ArgumentException("Using collectable function on non-collectable resource");

        if (change.Amount < 0)
            throw new ArgumentException("Collectable Resource Amount Change Cant be Negative");
        return true;
    }

    private static bool CheckExpiringNumeric(List<UserResource> userResources, Resource resource, ResourceChange change)
    {
        if (!ResourceType.Collectable.Equals(resource.Type))
            throw new ArgumentException("Using collectable function on non-collectable resource");

        ArgumentNullException.ThrowIfNull(change.Amount, "resourceChange.Amount != null");
        if (change is { Amount: < 0, Duration: not null })
            throw new ArgumentException("Cant Change Expiring Resource Expire Date");

        var userResource = userResources.Count == 0 ? null : userResources[0];

        if (change.Amount >= 0)
            return true;
        if (userResource is null)
            return false;

        var validUserResources = userResources.Where(ur => !ur.IsExpired).ToList();
        var isExpired = !validUserResources.Any();
        var cumulativeAmount = validUserResources.Sum(ur => ur.Amount);

        if (isExpired)
            return false;

        return cumulativeAmount + userResource.Amount >= 0;
    }

    #endregion
}