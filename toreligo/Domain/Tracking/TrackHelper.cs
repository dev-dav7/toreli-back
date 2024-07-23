using System.Text.Json;
using toreligo.Domain.Tracking.Entity;
using toreligo.Domain.Tracking.Models;

namespace toreligo.Domain.Tracking;

public static class TrackHelper
{
    public static TrackSerializedModel ToTrackSerialisedModel(this Track track) =>
        new ()
        {
            Id = track.Id,
            GroupId = track.GroupId,
            Params = track.Params,
            Options = track.Options
        };
}