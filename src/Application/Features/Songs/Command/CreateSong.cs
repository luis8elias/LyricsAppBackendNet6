using AutoMapper;

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

namespace LyricsApp.Application.Features.Songs.Command
{
    public class CreateSong : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/songs", async (SongRequest request, IMediator mediator) => await mediator.Send(request))
            .WithName(nameof(CreateSong))
            .WithTags(nameof(Song))
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status201Created, typeof(BasicResponse<Song>))
            .AddEndpointFilter<ValidationFilter<SongRequest>>()
            .RequireAuthorization();
        }

        public class SongRequest : IRequest<IResult>
        {
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

        public class CreateSongHandler : IRequestHandler<SongRequest, IResult>
        {
            private readonly ApiDbContext _context;
            private readonly IMapper _mapper;

            public CreateSongHandler(ApiDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<IResult> Handle(SongRequest request, CancellationToken cancellationToken)
            {
                try
                {
                    var song = new Song(request.Title, request.Lyric, request.GenreId);
                    foreach (var tag in request.Tags)
                    {
                        song.AddTag(new Tag(tag));
                    }
                    _context.Songs.Add(song);
                    await _context.SaveChangesAsync(cancellationToken);

                    return Results.CreatedAtRoute(nameof(CreateSong), new { song.Id },
                    new BasicResponse<Song>(true, "Song Successfully created ", song));
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new BasicResponse<Song>(false, ex.Message, null));
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new BasicResponse<Song>(false, "Something went wrong...", null));
                }
            }
        }
    }
}