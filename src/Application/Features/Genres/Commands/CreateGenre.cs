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


namespace LyricsApp.Application.Features.Genres.Commands
{
    public class CreateGenre : ICarterModule

    {

        public void AddRoutes(IEndpointRouteBuilder app)
        {

            app.MapPost("api/genres", async (CreateGenreCommand command, IMediator mediator) =>
            {
                return await mediator.Send(command);
            })
            .WithName(nameof(CreateGenre))
            .WithTags(nameof(Genre))
            .ProducesValidationProblem()
            .Produces(
                StatusCodes.Status201Created,
                typeof(BasicResponse<int>)
            )
            .AddEndpointFilter<ValidationFilter<CreateGenreCommand>>()
            .RequireAuthorization();
        }
    }

    public class CreateGenreCommand : IRequest<IResult>
    {
        public string Name { get; set; } = string.Empty;

    }

    public class CreateGenreValidator : AbstractValidator<CreateGenreCommand>
    {
        public CreateGenreValidator()
        {
            RuleFor(r => r.Name).NotEmpty().NotNull();
        }
    }

    public class CreateGenreHandler : IRequestHandler<CreateGenreCommand, IResult>
    {
        private readonly ApiDbContext _context;
        private readonly IValidator<CreateGenreCommand> _validator;

        public CreateGenreHandler(ApiDbContext context, IValidator<CreateGenreCommand> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<IResult> Handle(CreateGenreCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var newGenre = new Genre(request.Name);
                
                _context.Genres.Add(newGenre);

                await _context.SaveChangesAsync(cancellationToken);
                
                return Results.CreatedAtRoute(nameof(CreateGenre), new { newGenre.Id },
                    new BasicResponse<Genre>(true, "Genre creado correctamente", newGenre));

            }
            catch (Exception)
            {
                return Results.BadRequest(new BasicResponse<int?>(false, "Genre no creado", null));

            }
        }
    }
}
