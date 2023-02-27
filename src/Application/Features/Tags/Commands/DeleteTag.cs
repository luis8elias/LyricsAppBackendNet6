using Carter;
using LyricsApp.Application.Common.Models;
using LyricsApp.Application.Domain.Entities;
using LyricsApp.Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LyricsApp.Application.Features.Tags.Commands
{
    public class DeleteTag : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/tags/{tagId}", async (IMediator mediator, Guid tagId) =>
            {
                return await mediator.Send(new DeleteTagCommand(tagId));
            })
            .WithName(nameof(DeleteTag))
            .WithTags(nameof(Tag))
            .Produces(StatusCodes.Status200OK, typeof(BasicResponse<>))
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status404NotFound, typeof(BasicResponse<>));
        }
    }

    public class DeleteTagCommand : IRequest<IResult>
    {
        public DeleteTagCommand(Guid tagId) => TagId = tagId;

        public Guid TagId { get; set; }
    }

    public class DeleteProductHandler : IRequestHandler<DeleteTagCommand, IResult>
    {
        private readonly ApiDbContext _context;

        public DeleteProductHandler(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<IResult> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
        {
            try
            {

                var tag = await _context.Tags.FindAsync(request.TagId);

                if (tag is null)
                {
                    return Results.NotFound(new BasicResponse<object>(false, "Tag no encontrado", null));
                }

                _context.Tags.Remove(tag);

                await _context.SaveChangesAsync();

                return Results.Ok(new BasicResponse<object>(true, "Tag eliminado", null));

            }
            catch (Exception)
            {
                return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
