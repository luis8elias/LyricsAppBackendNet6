using AutoMapper;

using Carter;

using LyricsApp.Application.Common.Models;
using LyricsApp.Application.Domain.Entities;
using LyricsApp.Application.Domain.Interfaces;
using LyricsApp.Application.Features.Songs.Mapping;
using LyricsApp.Application.Infrastructure.Persistence;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LyricsApp.Application.Features.Songs.Queries
{
    public class GetSongById : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/songs/{songId}", async (Guid songId, IMediator mediator) => await mediator.Send(new SongRequest(songId)))
            .WithName(nameof(GetSongById))
            .Produces(StatusCodes.Status200OK, typeof(BasicResponse<SongDto?>))
            .Produces(StatusCodes.Status404NotFound, typeof(BasicResponse<SongDto?>))
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(nameof(Song))
            .RequireAuthorization();
        }
    }

    public class SongRequest : IRequest<IResult>
    {
        public Guid Id { get; init; }

        public SongRequest(Guid id)
        {
            Id = id;
        }
    }

    public class SongByIdHandler : IRequestHandler<SongRequest, IResult>
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextService _httpContextService;

        public SongByIdHandler(ApiDbContext context, IMapper mapper, IHttpContextService httpContextService)
        {
            _context = context;
            _mapper = mapper;
            _httpContextService = httpContextService;
        }

        public async Task<IResult> Handle(SongRequest request, CancellationToken cancellationToken)
        {
            var song = await _context.Songs.FirstOrDefaultAsync(x => x.Id == request.Id && x.CreatedBy == _httpContextService.UserId, cancellationToken);

            if (song == default)
            {
                return Results.NotFound(new BasicResponse<SongDto?>(false, "Song not found", null));
            }

            var songDto = _mapper.Map<SongDto>(song);
            return Results.Ok(new BasicResponse<SongDto>(true, "Song details", songDto));
        }
    }
}