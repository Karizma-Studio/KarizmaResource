using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using KarizmaPlatform.Core.Domain.Models;

namespace KarizmaPlatform.Resources.Domain.Models;

[Table("user_resources")]
public class UserResource : BaseEntity
{
    [Column("user_id"), Required] public long UserId { get; init; }
    [Column("resource_id"), Required] public long ResourceId { get; init; }
    [Column("amount")] public int? Amount { get; set; }
    [Column("expire_date")] public DateTimeOffset? ExpireDate { get; set; }
    [Column("collectable_id")] public long? CollectableId { get; set; }

    [JsonIgnore] public Resource? Resource { get; init; }
    [JsonIgnore] public IResourceUser? User { get; init; }

    public bool IsExpired => ExpireDate is not null && ExpireDate < DateTimeOffset.UtcNow;
}