using LinqToDB;
using LinqToDB.Mapping;
using toreligo.Domain.Database;

#pragma warning disable CS8618

namespace toreligo.Domain.Group.Entities;

[Table("group_track")]
public class GroupTrack : EntityWithId
{
    [Column("id_owner")] public long OwnerId { get; set; }

    [Column("name_group", Length = DescriptionLength)]
    public string Name { get; set; }

    [Column("removed_at", DataType = DataType.Timestamp)]
    public DateTime? RemovedTime { get; set; }

    public const int DescriptionLength = 50;
}