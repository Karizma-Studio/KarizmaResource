using KarizmaPlatform.Resources.Domain.Models;
using KarizmaPlatform.Resources.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KarizmaPlatform.Resources.Infrastructure.Repositories;

public class UserResourceRepository(IResourceDatabase resourceDatabase) : IUserResourceRepository
{
    public async Task<UserResource> Add(UserResource userResource)
    {
        resourceDatabase.GetUserResources().Add(userResource);
        await resourceDatabase.SaveChangesAsync();
        return userResource;
    }

    public Task Update(UserResource userResource)
    {
        resourceDatabase.GetUserResources().Update(userResource);
        return resourceDatabase.SaveChangesAsync();
    }

    public async Task DeleteById(long identifier)
    {
        var byId = await FindById(identifier);
        if (byId is null)
            return;
        resourceDatabase.GetUserResources().Remove(byId);
        await resourceDatabase.SaveChangesAsync();
    }

    public async Task SoftDeleteById(long identifier)
    {
        var byId = await FindById(identifier);
        if (byId is null)
            return;
        byId.DeletedDate = DateTimeOffset.UtcNow;
        resourceDatabase.GetUserResources().Update(byId);
        await resourceDatabase.SaveChangesAsync();
    }

    public Task<UserResource?> FindById(long identifier)
    {
        return resourceDatabase.GetUserResources().SingleOrDefaultAsync(x => x.Id == identifier);
    }

    public Task<UserResource?> FindNotDeletedById(long identifier)
    {
        return resourceDatabase.GetUserResources().SingleOrDefaultAsync((x => x.DeletedDate == new DateTimeOffset?() && x.Id == identifier));
    }

    public Task<List<UserResource>> GetAll()
    {
        return resourceDatabase.GetUserResources().ToListAsync();
    }

    public Task<List<UserResource>> GetAllNotDeleted()
    {
        return resourceDatabase.GetUserResources().Where(x => x.DeletedDate == new DateTimeOffset?()).ToListAsync();
    }

    public Task<List<UserResource>> FindUserResources(long userId, bool tracking)
    {
        return (tracking
                ? resourceDatabase.GetUserResources()
                : resourceDatabase.GetUserResources().AsNoTracking())
            .Include(ur => ur.Resource)
            .Where(ur => ur.UserId == userId)
            .ToListAsync();
    }

    public Task<List<UserResource>> FindUserResource(long userId, long resourceId, bool tracking = true)
    {
        return (tracking
                ? resourceDatabase.GetUserResources()
                : resourceDatabase.GetUserResources().AsNoTracking())
            .Include(ur => ur.Resource)
            .Where(ur => ur.UserId == userId && ur.ResourceId == resourceId)
            .OrderBy(ur => ur.ExpireDate)
            .ToListAsync();
    }

    public Task<UserResource?> FindUserCollectable(long userId, long resourceId, long collectableId, bool tracking = true)
    {
        return (tracking
                ? resourceDatabase.GetUserResources()
                : resourceDatabase.GetUserResources().AsNoTracking())
            .Include(ur => ur.Resource)
            .SingleOrDefaultAsync(ur => ur.UserId == userId && ur.ResourceId == resourceId && ur.CollectableId == collectableId);
    }
}