using toreligo.Domain.Database;
using toreligo.Domain.Tracking.Entity;

namespace toreligo.Domain.Tracking;

public class TrackRepository
{
    private readonly EntityStorage entityStorage;

    public TrackRepository(EntityStorage entityStorage)
    {
        this.entityStorage = entityStorage;
    }

    public IQueryable<Track> UserTracks(long userId)
        => entityStorage.Select<Track>()
            .Where(x => x.OwnerId == userId)
            .Where(x => x.RemovedTime == null);

    public IQueryable<Track> FindTrackForUser(long userId, long trackId)
        => UserTracks(userId)
            .Where(x => x.Id == trackId);

    public IQueryable<TrackFill> TrackFillQuery(long userId, long trackId) =>
        from t in entityStorage.Select<Track>()
        join f in entityStorage.Select<TrackFill>()
            on t.Id equals f.TrackId
        where t.OwnerId == userId
        where t.RemovedTime == null
        where t.Id == trackId
        select f;
}