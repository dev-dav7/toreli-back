using toreligo.Domain.Database;
using toreligo.Domain.Group.Entities;

namespace toreligo.Domain.Group;

public class GroupRepository
{
    private readonly EntityStorage entityStorage;

    public GroupRepository(EntityStorage entityStorage)
    {
        this.entityStorage = entityStorage;
    }

    public IQueryable<GroupTrack> UserGroups(long userId)
        => entityStorage.Select<GroupTrack>()
            .Where(x => x.OwnerId == userId)
            .Where(x => x.RemovedTime == null);

    public IQueryable<GroupTrack> FindUserForGroup(long userId, long groupId)
        => UserGroups(userId)
            .Where(x => x.Id == groupId);
}