namespace toreligo.Domain.Group.Models;
#pragma warning disable CS8618

public class CreateGroupRequest
{
    public string Name { get; set; }
}

public class GroupModel
{
    public long Id { get; set; }
    public string Name { get; set; }
}