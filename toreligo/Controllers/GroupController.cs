using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using toreligo.Domain.Database;
using toreligo.Domain.Group;
using toreligo.Domain.Group.Entities;
using toreligo.Domain.Group.Models;
using toreligo.Domain.Tracking.Entity;

namespace toreligo.Controllers;

[ApiController]
[Authorize]
[Route("api/group")]
public class GroupController : ControllerBase
{
    private readonly EntityStorage entityStorage;
    private readonly GroupRepository groupRepository;

    public GroupController(EntityStorage entityStorage, GroupRepository groupRepository)
    {
        this.entityStorage = entityStorage;
        this.groupRepository = groupRepository;
    }

    [HttpGet("getAll")]
    public IActionResult GetGroup()
    {
        var userId = WebHelper.GetUserId(User);
        var groups = groupRepository
            .UserGroups(userId)
            .ToArray()
            .Select(x => x.ToGroupModel());

        return Ok(groups);
    }

    [HttpPost("create")]
    public IActionResult CreateGroup(CreateGroupRequest request)
    {
        if (request.Name.Length is > GroupTrack.DescriptionLength or < 1)
            return BadRequest("Некорректное название группы.");

        var userId = WebHelper.GetUserId(User);
        var newGroup = entityStorage.CreateEntity(new GroupTrack
        {
            Name = request.Name,
            OwnerId = userId
        });

        return Ok(newGroup.ToGroupModel());
    }

    [HttpGet("remove/{groupId}")]
    public IActionResult RemoveGroup(long groupId)
    {
        var userId = WebHelper.GetUserId(User);
        var group = groupRepository.FindUserForGroup(userId, groupId).SingleOrDefault();
        if (group == default)
            return Ok();

        entityStorage.Update<Track>(x => x.GroupId == groupId, _ => new Track
        {
            GroupId = null
        });

        entityStorage.Update<GroupTrack>(x => x.Id == groupId, _ =>
            new GroupTrack
            {
                RemovedTime = DateTime.Now
            });
        
        return Ok();
    }
}