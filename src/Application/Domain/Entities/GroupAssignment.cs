using LyricsApp.Application.Domain.Base;

namespace LyricsApp.Application.Domain.Entities;

public class GroupAssignment : EntityTracking
{
    private GroupAssignment() { }
    
    public GroupAssignment(Guid groupId, Guid userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        GroupId = groupId;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid GroupId { get; private set; }

    public User User { get; private set; }
    public Group Group { get; private set; }
}