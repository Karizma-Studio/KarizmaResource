using KarizmaPlatform.Resources.Domain.Models;

namespace KarizmaPlatform.Resources.Application.Services;

public class ResourceCache<T> where T : notnull
{
    private Dictionary<T, Resource> resourceDictionary = new();

    public void Populate(Dictionary<T, Resource> data)
    {
        resourceDictionary = data;
    }

    public Resource GetResource(T label)
    {
        return resourceDictionary[label];
    }

    public List<Resource> GetAll()
    {
        return resourceDictionary.Values.ToList();
    }
}