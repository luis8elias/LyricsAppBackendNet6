using AutoMapper;
using AutoMapper.QueryableExtensions;
using Carter;
using LyricsApp.Application.Common.Models;
using LyricsApp.Application.Domain.Entities;
using LyricsApp.Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LyricsApp.Application.Features.Genres.Queries
{
    public class GetGenreById : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/genres/{genreId}", (IMediator mediator, string genreId) =>
            {
                return mediator.Send(new GetGenreByIdQuery(genreId));
            })
            .WithName(nameof(GetGenreById))
            .Produces(StatusCodes.Status200OK, typeof(BasicResponse<GenreResponse>))
            .Produces(StatusCodes.Status404NotFound, typeof(BasicResponse<>))
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(nameof(Genre))
            .RequireAuthorization();
        }

        public record GetGenreByIdQuery(string genreId) : IRequest<IResult>;

        public class GetGenreByIdHandler : IRequestHandler<GetGenreByIdQuery, IResult>
        {
            private readonly ApiDbContext _context;
            private readonly IMapper _mapper;

            public GetGenreByIdHandler(ApiDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<IResult> Handle(GetGenreByIdQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var genreGuid = Guid.Parse(request.genreId);
                    var genre = await _context.Genres.FirstOrDefaultAsync(genre => genre.Id == genreGuid);
                    if (genre is null)
                    {
                        return Results.NotFound(new BasicResponse<GenreResponse?>(false, "Género no encontrado", null));
                    }
                    var genreDetails = _mapper.Map<GenreResponse>(genre);
                    return Results.Ok(new BasicResponse<GenreResponse>(true, "Detalles del Género", genreDetails));
                }
                catch (Exception)
                {

                    return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
                }

            }
        }

       
    }

}