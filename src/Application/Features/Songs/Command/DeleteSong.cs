using Carter;

using LyricsApp.Application.Common.Models;
using LyricsApp.Application.Domain.Entities;
using LyricsApp.Application.Domain.Interfaces;
using LyricsApp.Application.Infrastructure.Persistence;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LyricsApp.Application.Features.Songs.Command
{
    public class DeleteSong : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/songs/{songId}", async (Guid songId, IMediator mediator) => await mediator.Send(new DeleteSongRequest(songId)))
            .WithName(nameof(DeleteSong))
            .WithTags(nameof(Song))
            .Produces(StatusCodes.Status200OK, typeof(BasicResponse<>))
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status404NotFound, typeof(BasicResponse<>));
        }
    }

    public class DeleteSongRequest : IRequest<IResult>
    {
        public Guid Id { get; init; }

        public DeleteSongRequest(Guid id)
        {
            Id = id;
        }
    }

    public class DeleteSongHandler : IRequestHandler<DeleteSongRequest, IResult>
    {
        private readonly ApiDbContext _context;
        private readonly IHttpContextService _httpContextService;

        public DeleteSongHandler(ApiDbContext context, IHttpContextService httpContextService)
        {
            _context = context;
            _httpContextService = httpContextService;
        }
        public async Task<IResult> Handle(DeleteSongRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var existingSong = await _context.Songs.FirstOrDefaultAsync(x => x.Id == request.Id && x.CreatedBy == _httpContextService.UserId, cancellationToken);
    
                if(existingSong == default)
                {
                    return Results.NotFound(new BasicResponse<Song?>(false, "Song not found", null));
                }
    
                _context.Songs.Remove(existingSong);
                await _context.SaveChangesAsync(cancellationToken);
    
                return Results.Ok(new BasicResponse<Song>(true, "The song was successfully deleted", null));
            }
            catch (System.Exception)
            {
                
                throw;
            }
        }
    }
}