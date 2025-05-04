using System;
using System.Text.Json;
using KarizmaPlatform.Resources.SharedClasses.JsonSchemas;

namespace KarizmaPlatform.Resources.SharedClasses.Dtos
{
    public class ResourceLogEventArgs : EventArgs
    {
        public long UserId { get; set; }
        public string Purpose { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public ResourceChange ResourceChange { get; set; }
        public JsonDocument Balance { get; set; }
        public JsonDocument? Meta { get; set; }
    }
}