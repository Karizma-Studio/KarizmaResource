using System.Text.Json;

namespace KarizmaPlatform.Resources.SharedClasses.Dtos
{
    public class ResourceChangeInfo
    {
        public string Purpose { get; set; } = null!;
        public JsonDocument? Meta { get; set; }
    }
}