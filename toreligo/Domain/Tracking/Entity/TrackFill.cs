using LinqToDB;
using LinqToDB.Mapping;
using toreligo.Domain.Database;

#pragma warning disable CS8618

namespace toreligo.Domain.Tracking.Entity;

[Table("track_fill")]
public class TrackFill : EntityWithId
{
    [Column("id_track")] public long TrackId { get; set; }

    [Column("day")] public int Day { get; set; }

    [Column("day_e")] public int DayE { get; set; }

    [Column("values", DataType = DataType.Json)]
    public string Values { get; set; }
}