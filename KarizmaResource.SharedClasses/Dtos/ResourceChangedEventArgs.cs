using System;
using KarizmaPlatform.Resources.SharedClasses.JsonSchemas;

namespace KarizmaPlatform.Resources.SharedClasses.Dtos
{
    public class ResourceChangedEventArgs : EventArgs
    {
        public long UserId { get; set; }
        public ResourceChange ResourceChange { get; set; }
    }
}