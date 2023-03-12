using Carter;

using LyricsApp.Application.Common.Models;
using LyricsApp.Application.Domain.Interfaces;
using LyricsApp.Application.Features.Auth.Models;
using LyricsApp.Application.Infrastructure.Persistence;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LyricsApp.Application.Features.Auth.Commands
{
    public class RefreshToken : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("auth/RefreshToken", async (RefreshTokenRequest request, IMediator mediator) => await mediator.Send(request))
            .WithName(nameof(RefreshToken))
            .WithTags("Auth")
            .Produces(
                StatusCodes.Status200OK,
                typeof(BasicResponse<TokenResponse>)
            )
            .Produces(StatusCodes.Status400BadRequest, typeof(BasicResponse<TokenResponse?>));
        }

        public class RefreshTokenRequest : IRequest<IResult>
        {
            public RefreshTokenRequest(string refreshToken)
            {
                RefreshToken = refreshToken;
            }

            public string RefreshToken { get; set; }
        }

        public class RefreshTokenHandler : IRequestHandler<RefreshTokenRequest, IResult>
        {
            private readonly ApiDbContext _context;
            private readonly IAuthService _authService;
            private readonly IHttpContextService _httpContextService;

            public RefreshTokenHandler(ApiDbContext context, IAuthService authService, IHttpContextService httpContextService)
            {
                _context = context;
                _authService = authService;
                _httpContextService = httpContextService;
            }

            public async Task<IResult> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
            {
                var refreshToken = request.RefreshToken;

                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    refreshToken = _httpContextService.GetCookie("refreshToken");
                }

                if (refreshToken is null)
                {
                    return Results.BadRequest(new BasicResponse<TokenResponse?>(false, "The refresh token is invalid", null));
                }

                var user = await _context.Users.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken, cancellationToken: cancellationToken);

                if (user is null)
                {
                    return Results.BadRequest(new BasicResponse<TokenResponse?>(false, "The refresh token is invalid", null));
                }

                if (user.RefreshTokenExpires < DateTime.Now)
                {
                    return Results.BadRequest(new BasicResponse<TokenResponse?>(false, "The refresh token is invalid", null));
                }

                var jwt = _authService.GenerateJwt(user);
                _authService.GenerateRefreshToken(user);

                _context.Users.Update(user);
                await _context.SaveChangesAsync(cancellationToken);

                TokenResponse jwtResponse = new(jwt, user.RefreshToken);

                return Results.Ok(new BasicResponse<TokenResponse>(true, "Refresh Token Success", jwtResponse));
            }
        }
    }
}