using Carter;
using FluentValidation;
using LyricsApp.Application.Common.Filters;
using LyricsApp.Application.Common.Models;
using LyricsApp.Application.Domain.Entities;
using LyricsApp.Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;


namespace LyricsApp.Application.Features.Auth.Commands
{
    public class RegisterUser : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("auth/register-user", async (RegisterUserCommand command, IMediator mediator) =>
            {
                return await mediator.Send(command);
            })
            .WithName(nameof(RegisterUser))
            .WithTags("Auth")
            .ProducesValidationProblem()
            .Produces(
                StatusCodes.Status201Created,
                typeof(BasicResponse<User>)
            )
            .AddEndpointFilter<ValidationFilter<RegisterUserCommand>>(); ;
        }
    }

    public class RegisterUserCommand : IRequest<IResult>
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = null;

    }

    public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserValidator()
        {
            RuleFor(r => r.Name).NotEmpty().NotNull();
            RuleFor(r => r.Email).EmailAddress();
            RuleFor(r => r.Password).NotEmpty().NotNull();
        }
    }

    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, IResult>
    {
        private readonly ApiDbContext _context;

        public RegisterUserHandler(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<IResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                var newUser = new User(0, request.Name,request.Email, passwordHash, request.PhoneNumber);
                
                _context.Users.Add(newUser);

                await _context.SaveChangesAsync(cancellationToken);

                return Results.CreatedAtRoute(nameof(RegisterUser), new { newUser.Id },
                    new BasicResponse<User>(true, "Usuario creado correctamente", newUser));

            }
            catch (Exception)
            {
                return Results.BadRequest(new BasicResponse<User?>(false, "Usuario no creado", null));

            }
        }
    }

}
