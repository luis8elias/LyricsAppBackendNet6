using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Carter;

using FluentValidation;

using LyricsApp.Application.Common.Filters;
using LyricsApp.Application.Common.Models;
using LyricsApp.Application.Domain.Entities;
using LyricsApp.Application.Domain.Interfaces;
using LyricsApp.Application.Features.Auth.Models;
using LyricsApp.Application.Infrastructure.Persistence;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LyricsApp.Application.Features.Auth.Commands;

public class Login : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/Login", async ([FromBody] LoginRequest request, IMediator mediator) => await mediator.Send(request))
        .WithName(nameof(Login))
        .WithTags("Auth")
        .ProducesValidationProblem()
        .Produces(
            StatusCodes.Status200OK,
            typeof(BasicResponse<TokenResponse>)
        )
        .AddEndpointFilter<ValidationFilter<LoginRequest>>();
    }

    public record LoginRequest(string Username, string Password) : IRequest<IResult>;

    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Username).NotNull().WithMessage("Username is required");
            RuleFor(x => x.Password).NotNull().WithMessage("Password is required");
        }
    }

    public class LoginHandler : IRequestHandler<LoginRequest, IResult>
    {
        private readonly ApiDbContext _context;
        private readonly IAuthService _authService;
        private readonly IHttpContextService _httpContextService;

        public LoginHandler(ApiDbContext context, IAuthService authService, IHttpContextService httpContextService)
        {
            _context = context;
            _authService = authService;
            _httpContextService = httpContextService;
        }


        public async Task<IResult> Handle(LoginRequest request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == request.Username, cancellationToken: cancellationToken);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Results.BadRequest(new BasicResponse<string>(false, "Error", "Username and/or Password are invalid"));
            }

            var jwt = _authService.GenerateJwt(user);
            _authService.GenerateRefreshToken(user);

            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);

            var jwtResponse = new TokenResponse(jwt, user.RefreshToken);

            return Results.Ok(new BasicResponse<TokenResponse>(true, "Login Success", jwtResponse));

        }
    }    
}