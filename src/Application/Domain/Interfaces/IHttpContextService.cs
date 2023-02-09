namespace LyricsApp.Application.Domain.Interfaces;

public interface IHttpContextService
{
    Guid UserId { get; }
}