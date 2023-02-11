namespace LyricsApp.Application.Features.Groups;


public record GroupResponse(Guid Id, string Name, string Code);

public class GroupDetailResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public IEnumerable<string> Members { get; set; }
}
