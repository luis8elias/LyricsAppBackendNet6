using LyricsApp.Application.Domain.Base;

namespace LyricsApp.Application.Domain.Entities;

public class Group :EntityTracking
{
    private Group()
    {}

    public Group(string name, Guid adminId)
    {
        Id = Guid.NewGuid();
        Name =  name;
        Code = Guid.NewGuid().ToString().Split('-')[0];
        AdminId = adminId;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }
    public Guid AdminId { get; private set; }

    public User Admin { get; private set; }

    public ICollection<GroupAssignment> Members { get; private set; }
}