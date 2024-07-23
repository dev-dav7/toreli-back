namespace toreligo.Domain.Tracking.Models;
#pragma warning disable CS8618

public class FillTrackRequest
{
    public long TrackId { get; set; }
    public int Day { get; set; }
    public int DayE { get; set; }
    public IEnumerable<TrackValue> Values { get; set; }
}

public class TrackValue
{
    public int Id { get; set; }
    public IEnumerable<OptionValue> Values { get; set; }
}

public class OptionValue
{
    public int Id { get; set; }
    public object Value { get; set; }
}

public class ValuesModel
{
    public long TrackId { get; set; }
    public int Day { get; set; }
    public int DayE { get; set; }
    public IEnumerable<TrackValue> Values { get; set; }
}

public class TrackSerializedModel
{
    public long Id { get; set; }
    public long? GroupId { get; set; }
    public string Params { get; set; }
    public string Options { get; set; }
}

public class TrackFillsModel
{
    public long TrackId { get; set; }
    public IEnumerable<TrackFillModel> Values { get; set; }
}

public class TrackFillModel
{
    public int Day { get; set; }
    public int DayE { get; set; }
    public IEnumerable<TrackValue> Values { get; set; }
}

public class SetValueResponse
{
    public int RemoveFrom { get; set; }
    public int RemoveTo { get; set; }
    public IEnumerable<TrackFillModel> Add { get; set; }
}