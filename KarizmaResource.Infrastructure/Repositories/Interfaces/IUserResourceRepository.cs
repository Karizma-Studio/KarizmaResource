using KarizmaPlatform.Core.Logic;
using KarizmaPlatform.Resources.Domain.Models;

namespace KarizmaPlatform.Resources.Infrastructure.Repositories.Interfaces;

public interface IUserResourceRepository : IRepository<UserResource>
{
    Task<List<UserResource>> FindUserResources(long userId, bool tracking = true);
    Task<List<UserResource>> FindUserResource(long userId, long resourceId, bool tracking = true);
    Task<UserResource?> FindUserCollectable(long userId, long resourceId, long collectableId, bool tracking = true);
}