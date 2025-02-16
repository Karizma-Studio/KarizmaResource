using KarizmaPlatform.Resources.SharedClasses.JsonSchemas;

namespace KarizmaPlatform.Resources.Domain.Models;

public class ResourceChangedEventArgs : EventArgs
{
    public long UserId { get; set; }
    public ResourceChange ResourceChange { get; set; }
}