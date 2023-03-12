using Microsoft.AspNetCore.Http;

namespace LyricsApp.Application.Domain.Interfaces;

public interface IHttpContextService
{
    Guid UserId { get; }
    void SetCookie(string key, string value, CookieOptions options);
    string? GetCookie(string key);
}