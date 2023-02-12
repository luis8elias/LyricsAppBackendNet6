using LyricsApp.Application.Domain.Base;

namespace LyricsApp.Application.Domain.Entities;

public class Genre : EntityTracking
{
    protected Genre() { }

    public Genre(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }
    public Guid Id { get; private set; }
    public string Name { get; private set; }

    public void UpdateName(string name)
    {
        Name = name;
    }

    public ICollection<Song> Songs { get; private set; }
}