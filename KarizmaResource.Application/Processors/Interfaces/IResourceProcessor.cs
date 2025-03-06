using KarizmaPlatform.Resources.Domain.Models;
using KarizmaPlatform.Resources.SharedClasses.JsonSchemas;

namespace KarizmaPlatform.Resources.Application.Processors.Interfaces;

public interface IResourceProcessor<T>
{
    Resource GetResource(T resourceLabel);
    Task<bool> CanChange(long userId, ResourceChange change);
    Task<bool> AddTransaction(long userId, List<ResourceChange> resourceChanges);
}