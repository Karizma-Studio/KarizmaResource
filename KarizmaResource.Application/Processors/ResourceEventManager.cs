using KarizmaPlatform.Resources.Domain.Models;

namespace KarizmaPlatform.Resources.Application.Processors;

public class ResourceEventManager
{
    public event EventHandler<ResourceChangedEventArgs> ResourceChanged;

    public void OnResourceChanged(ResourceChangedEventArgs e)
    {
        ResourceChanged?.Invoke(this, e);
    }
}