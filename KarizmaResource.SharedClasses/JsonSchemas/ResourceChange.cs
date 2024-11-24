using System;

namespace KarizmaPlatform.Resources.SharedClasses.JsonSchemas
{
    public class ResourceChange
    {
        public string Title { get; set; } = string.Empty;
        public int? Amount { get; set; }
        public double? Duration { get; set; }
        public long? CollectableId { get; set; }

        public TEnum GetResourceEnum<TEnum>() where TEnum : struct, Enum
        {
            return Enum.Parse<TEnum>(Title);
        }
    }
}