using AutoMapper;
using Carter;
using FluentValidation;
using LyricsApp.Application.Common.Filters;
using LyricsApp.Application.Common.Models;
using LyricsApp.Application.Domain.Entities;
using LyricsApp.Application.Features.Tags.Commands;
using LyricsApp.Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LyricsApp.Application.Features.Genres.Commands
{
    public class UpdateGenre : ICarterModule
    {

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/genres", async (UpdateGenreCommand command, IMediator mediator) =>
            {
                return await mediator.Send(command);
            })
            .WithName(nameof(UpdateGenre))
            .WithTags(nameof(Genre))
            .Produces(StatusCodes.Status404NotFound)
            .Produces(
                StatusCodes.Status200OK,
                typeof(BasicResponse<GenreResponse?>)
            )
            .Produces(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .AddEndpointFilter<ValidationFilter<UpdateGenreCommand>>();

        }
    }

    public class UpdateGenreCommand : IRequest<IResult>
    {

        public string GenreId { get; set; }
        public string NewName { get; set; } = string.Empty;
    }

    public class UpdateGenreValidator : AbstractValidator<UpdateGenreCommand>
    {
        public UpdateGenreValidator()
        {
            RuleFor(r => r.GenreId).NotEmpty().NotNull();
            RuleFor(r => r.NewName).NotEmpty().NotNull();
        }
    }

    public class UpdateGenreHandler : IRequestHandler<UpdateGenreCommand, IResult>
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;


        public UpdateGenreHandler(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IResult> Handle(UpdateGenreCommand request, CancellationToken cancellationToken)
        {

            try
            {
                var genreGuid = Guid.Parse(request.GenreId);
                var genre = await _context.Genres.FirstOrDefaultAsync(genre => genre.Id == genreGuid);

                if (genre is null)
                {
                    return Results.NotFound(new BasicResponse<GenreResponse?>(false, "Género no encontrado", null));
                }
                genre.Name = request.NewName;
                await _context.SaveChangesAsync();
                var genreDetails = _mapper.Map<GenreResponse>(genre);

                return Results.Ok(new BasicResponse<GenreResponse>(true, "Género actualizado", genreDetails));

            }
            catch (Exception)
            {
                return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }

}
