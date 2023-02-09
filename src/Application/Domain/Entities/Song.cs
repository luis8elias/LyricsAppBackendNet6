using LyricsApp.Application.Domain.Base;

namespace LyricsApp.Application.Domain.Entities;

public class Song : EntityTracking
{
    private Song() { }
    
    public Song(string title, string lyric, Genre genre)
    {
        Id = Guid.NewGuid();
        Title = title;
        Lyric = lyric;
        GenreId = genre.Id;
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Lyric { get; private set; }
    public Guid GenreId { get; private set; }

    public Genre Genre { get; private set; }
}
