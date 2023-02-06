using System.Security.Claims;
using LyricsApp.Application.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LyricsApp.Application.Services;

public class HttpContextService : IHttpContextService
{
    private readonly IHttpContextAccessor contextAccessor;

    public HttpContextService(IHttpContextAccessor contextAccessor)
    {
        this.contextAccessor = contextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userId = contextAccessor?.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid);

            if ( userId == null)
            {
                return Guid.Empty;
            }

            return new Guid(userId.Value);

        }
    }
}