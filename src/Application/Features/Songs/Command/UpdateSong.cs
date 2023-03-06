using AutoMapper;

using Carter;

using FluentValidation;

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

namespace LyricsApp.Application.Features.Songs.Command
{
    public class EditSong : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/song/{songId}", async (Guid songId, SongRequest request, IMediator mediator) => {
                request.Id = songId;
                return await mediator.Send(request);
            })
            .WithName(nameof(EditSong))
            .WithTags(nameof(Song))
            .Produces(StatusCodes.Status404NotFound)
            .Produces(
                StatusCodes.Status200OK,
                typeof(BasicResponse<SongDto?>)
            )
            .Produces(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem();
        }
    }


    public class SongRequest : IRequest<IResult>
    {
        public Guid Id { get; set; }
        public string Title { get; init; }
        public string Lyric { get; init; }
        public Guid GenreId { get; init; }
        public IList<string> Tags { get; set; }
    }

    public class SongRequestValidations : AbstractValidator<SongRequest>
    {
        public SongRequestValidations()
        {
            RuleFor(x => x.Title)
                .NotNull()
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Lyric)
                .NotNull()
                .NotEmpty();

            RuleFor(x => x.GenreId)
                .NotNull()
                .NotEmpty();
        }
    }


    public class UpdateSongHandler : IRequestHandler<SongRequest, IResult>
    {
        private readonly ApiDbContext _context;
        private readonly IHttpContextService _httpContextService;
        private readonly IMapper _mapper;

        public UpdateSongHandler(ApiDbContext context, IHttpContextService httpContextService, IMapper mapper)
        {
            _context = context;
            _httpContextService = httpContextService;
            _mapper = mapper;
        }
        public async Task<IResult> Handle(SongRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var existingSong = await _context.Songs.FirstOrDefaultAsync(x => x.CreatedBy == _httpContextService.UserId && x.Id == request.Id, cancellationToken);

                if (existingSong == default)
                {
                    return Results.NotFound(new BasicResponse<SongDto?>(false, "Song not found", null));
                }

                if(
                    existingSong.UpdateTitle(request.Title) ||
                    existingSong.UpdateLyric(request.Lyric) ||
                    existingSong.UpdateGenre(request.GenreId) ||
                    existingSong.UpdateTags(request.Tags)
                )
                {
                    _context.Songs.Update(existingSong);
                    await _context.SaveChangesAsync(cancellationToken);

                    var songDto = _mapper.Map<SongDto>(existingSong);
                    return Results.Ok(new BasicResponse<SongDto>(true, "Song updated successfully", songDto));
                }

                return Results.Ok(new BasicResponse<SongDto>(true, "Noting to update", null));
            }
            catch (System.Exception ex)
            {
                throw;
            }
        }
    }
}