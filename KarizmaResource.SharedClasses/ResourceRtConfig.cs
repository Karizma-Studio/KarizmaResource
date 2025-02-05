using KarizmaPlatform.Resources.SharedClasses.Enums;
using KarizmaPlatform.Resources.SharedClasses.JsonSchemas;
using Reinforced.Typings.Fluent;

namespace KarizmaPlatform.Resources.SharedClasses
{
    public class ResourceRtConfig
    {
        public static void Configure(ConfigurationBuilder builder)
        {
            builder.ExportAsClass<ResourceChange>()
                .WithPublicProperties();

            builder.ExportAsEnum<ResourceType>();
        }
    }
}