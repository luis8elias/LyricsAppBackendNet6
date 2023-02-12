using Carter;
using LyricsApp.Application.Common.Models;
using LyricsApp.Application.Domain.Entities;
using LyricsApp.Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;


namespace LyricsApp.Application.Features.Genres.Commands
{
    public class DeleteGenre : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/genre/{genreId}", async (IMediator mediator, Guid genreId) =>
            {
                return await mediator.Send(new DeleteGenreCommand(genreId));
            })
            .WithName(nameof(DeleteGenre))
            .WithTags(nameof(Genre))
            .Produces(StatusCodes.Status200OK, typeof(BasicResponse<>))
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status404NotFound, typeof(BasicResponse<>));
        }
    }

    public class DeleteGenreCommand : IRequest<IResult>
    {
        public DeleteGenreCommand(Guid genreId) => GenreId = genreId;

        public Guid GenreId { get; set; }
    }

    public class DeleteGenreHandler : IRequestHandler<DeleteGenreCommand, IResult>
    {
        private readonly ApiDbContext _context;

        public DeleteGenreHandler(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<IResult> Handle(DeleteGenreCommand request, CancellationToken cancellationToken)
        {
            try
            {
                
                var genre = await _context.Genres.FirstOrDefaultAsync(genre => genre.Id == request.GenreId);

                if (genre is null)
                {
                    return Results.NotFound(new BasicResponse<object>(false, "Género no encontrado", null));
                }

                _context.Genres.Remove(genre);

                await _context.SaveChangesAsync();

                return Results.Ok(new BasicResponse<object>(true, "Género eliminado", null));

            }
            catch (Exception)
            {
                return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
