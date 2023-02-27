using System.Security.Claims;
using LyricsApp.Application.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LyricsApp.Application.Services;

public class HttpContextService : IHttpContextService
{
    private readonly IHttpContextAccessor _contextAccessor;

    public HttpContextService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userId = _contextAccessor?.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid);

            return userId == null ? Guid.Empty : new Guid(userId.Value);
        }
    }
}