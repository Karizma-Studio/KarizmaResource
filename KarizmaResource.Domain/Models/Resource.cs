using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KarizmaPlatform.Core.Domain.Models;
using KarizmaPlatform.Resources.SharedClasses.Enums;

namespace KarizmaPlatform.Resources.Domain.Models;

[Table("resources")]
public class Resource : BaseEntity
{
    [Column("title"), Required, MaxLength(30)] public required string Title { get; init; }
    [Column("type"), Required] public required ResourceType Type { get; init; }
    [Column("category"), Required, MaxLength(20)] public required string Category { get; init; }

    public TEnum GetResourceEnum<TEnum>() where TEnum : struct, Enum
    {
        return Enum.Parse<TEnum>(Title);
    }
}