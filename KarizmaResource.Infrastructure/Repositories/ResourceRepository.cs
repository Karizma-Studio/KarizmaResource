using KarizmaPlatform.Resources.Domain.Models;
using KarizmaPlatform.Resources.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KarizmaPlatform.Resources.Infrastructure.Repositories;

public class ResourceRepository(IResourceDatabase resourceDatabase) : IResourceRepository
{
    public async Task<Resource> Add(Resource userResource)
    {
        resourceDatabase.GetResources().Add(userResource);
        await resourceDatabase.SaveChangesAsync();
        return userResource;
    }

    public Task Update(Resource userResource)
    {
        resourceDatabase.GetResources().Update(userResource);
        return resourceDatabase.SaveChangesAsync();
    }

    public async Task DeleteById(long identifier)
    {
        var byId = await FindById(identifier);
        if (byId is null)
            return;
        resourceDatabase.GetResources().Remove(byId);
        await resourceDatabase.SaveChangesAsync();
    }

    public async Task SoftDeleteById(long identifier)
    {
        var byId = await FindById(identifier);
        if (byId is null)
            return;
        byId.DeletedDate = DateTimeOffset.UtcNow;
        resourceDatabase.GetResources().Update(byId);
        await resourceDatabase.SaveChangesAsync();
    }

    public Task<Resource?> FindById(long identifier)
    {
        return resourceDatabase.GetResources().SingleOrDefaultAsync(x => x.Id == identifier);
    }

    public Task<Resource?> FindNotDeletedById(long identifier)
    {
        return resourceDatabase.GetResources().SingleOrDefaultAsync((x => x.DeletedDate == new DateTimeOffset?() && x.Id == identifier));
    }

    public Task<List<Resource>> GetAll()
    {
        return resourceDatabase.GetResources().ToListAsync();
    }

    public Task<List<Resource>> GetAllNotDeleted()
    {
        return resourceDatabase.GetResources().Where(x => x.DeletedDate == new DateTimeOffset?()).ToListAsync();
    }
}