using AutoMapper;
using AutoMapper.QueryableExtensions;
using Carter;
using LyricsApp.Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using LyricsApp.Application.Domain.Entities;
using LyricsApp.Application.Common.Models;

namespace LyricsApp.Application.Features.Genres.Queries;

public class GetGenres : ICarterModule

{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/genres", (IMediator mediator) =>
        {
            return mediator.Send(new GetGenreQuery());
        })
        .WithName(nameof(GetGenres))
        .Produces(StatusCodes.Status200OK,typeof(BasicResponse<List<GenreResponse>>))
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags(nameof(Genre))
        .RequireAuthorization();
    }

    public class GetGenreQuery : IRequest<IResult>
    {

    }

    public class GetGenresHandler : IRequestHandler<GetGenreQuery, IResult>
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;

        public GetGenresHandler(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IResult> Handle(GetGenreQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var genreList = await _context.Genres.ProjectTo<GenreResponse>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken: cancellationToken);
                return Results.Ok(new BasicResponse<List<GenreResponse>>(true, "Genres list", genreList));
            }
            catch (Exception)
            {

                return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
            }

        }
    }
  
}