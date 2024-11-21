using KarizmaPlatform.Resources.SharedClasses.Enums;

namespace KarizmaPlatform.Resources.Application.Services;

public struct ResourceStructure<T>
{
    public required T Title { get; set; }
    public required ResourceType Type { get; set; }
    public required string Category { get; set; }
}