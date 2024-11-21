using KarizmaPlatform.Core.Database;
using KarizmaPlatform.Resources.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace KarizmaPlatform.Resources.Infrastructure;

public interface IResourceDatabase : IBaseContext
{
    DbSet<UserResource> GetUserResources();
    DbSet<Resource> GetResources();
}