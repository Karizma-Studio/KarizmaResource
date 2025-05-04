using KarizmaPlatform.Resources.SharedClasses.Dtos;
using Microsoft.VisualBasic;

namespace KarizmaPlatform.Resources.Application.Processors;

public class ResourceEventManager
{
    public event EventHandler<ResourceChangedEventArgs> ResourceChanged;
    public event EventHandler<ResourceLogEventArgs> ResourceLog;

    public void OnResourceChanged(ResourceChangedEventArgs e)
    {
        ResourceChanged?.Invoke(this, e);
    }

    public void OnResourceLogged(ResourceLogEventArgs e)
    {
        ResourceLog?.Invoke(this, e);
    }
}