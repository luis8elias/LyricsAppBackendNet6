namespace LyricsApp.Application.Domain.Base;

public interface ISoftDelete
{
    public bool IsRemoved { get; set; }
}