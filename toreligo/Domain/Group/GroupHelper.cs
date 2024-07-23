using toreligo.Domain.Group.Entities;
using toreligo.Domain.Group.Models;

namespace toreligo.Domain.Group;

public static class GroupHelper
{
    public static GroupModel ToGroupModel(this GroupTrack groupTrack) =>
        new()
        {
            Id = groupTrack.Id,
            Name = groupTrack.Name,
        };
}