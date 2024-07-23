using LinqToDB;
using LinqToDB.Mapping;
using toreligo.Domain.Database;

#pragma warning disable CS8618

namespace toreligo.Domain.Tracking.Entity;

[Table("track")]
public class Track : EntityWithId
{
    [Column("id_owner")] public long OwnerId { get; set; }

    [Column("id_group")] public long? GroupId { get; set; }

    [Column("params", DataType = DataType.Json)]
    public string Params { get; set; }

    [Column("options", DataType = DataType.Json)]
    public string Options { get; set; }
    
    [Column("removed_at", DataType = DataType.Timestamp)]
    public DateTime? RemovedTime { get; set; }
}