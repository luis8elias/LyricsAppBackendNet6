namespace LyricsApp.Application.Features.Groups;


public record GroupResponse(Guid Id, string Name, string Code);

public class GroupDetailResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public IEnumerable<GroupMember>? Members { get; set; }
}

public class GroupMember {

    public GroupMember(Guid userId, string name)
    {
        UserId = userId;
        Name = name;
    }

    public Guid UserId { get; private set; }
    public string Name { get; private set; }
}
