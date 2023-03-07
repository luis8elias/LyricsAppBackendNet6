using AutoMapper;

using Carter;

using FluentValidation;

using LyricsApp.Application.Common.Filters;
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
    public class GetSongs : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/songs", async (IMediator mediator) => await mediator.Send(new GetSongRequest()))
            .WithName(nameof(GetSongs))
            .WithTags(nameof(Song))
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status200OK, typeof(BasicResponse<List<SongDto>>))
            .RequireAuthorization();
        }
    }

    public class GetSongRequest : IRequest<IResult>
    {
        public GetSongRequest()
        {
        }
    }

    public class GetSongRequestValidator : AbstractValidator<GetSongRequest>
    {
        public GetSongRequestValidator()
        {
        }
    }

    public class GetSongsHandler : IRequestHandler<GetSongRequest, IResult>
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextService _httpContextService;

        public GetSongsHandler(ApiDbContext context, IMapper mapper, IHttpContextService httpContextService)
        {
            _context = context;
            _mapper = mapper;
            _httpContextService = httpContextService;
        }

        public async Task<IResult> Handle(GetSongRequest request, CancellationToken cancellationToken)
        {
            var songs = await _context.Songs.Where(x => x.CreatedBy == _httpContextService.UserId).ToListAsync(cancellationToken: cancellationToken);

            var songsListDto = _mapper.Map <List<SongDto>>(songs);

            return Results.Ok(new BasicResponse<List<SongDto>>(true, "Songs", songsListDto));
        }
    }
}