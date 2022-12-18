using Carter;
using LyricsApp.Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using LyricsApp.Application.Domain.Entities;
using LyricsApp.Application.Common.Models;

namespace LyricsApp.Application.Features.Tags.Queries;

public class GetTagById : ICarterModule

{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/tags/{tagId}", (IMediator mediator ,int tagId) =>
        {
            return mediator.Send(new GetTagByIdQuery(tagId));
        })
        .WithName(nameof(GetTagById))
        .Produces(StatusCodes.Status200OK, typeof(BasicResponse<Tag>))
        .Produces(StatusCodes.Status404NotFound, typeof(BasicResponse<>))
        .WithTags(nameof(Tag));
    }

    public record GetTagByIdQuery(int TagId) : IRequest<IResult>;

    public class GetTagByIdHandler : IRequestHandler<GetTagByIdQuery, IResult>
    {
        private readonly ApiDbContext _context;

        public GetTagByIdHandler(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<IResult> Handle(GetTagByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var tag = await _context.Tags.FindAsync(request.TagId);

                if (tag is null)
                {
                    return Results.NotFound(new BasicResponse<Tag?>(false, "Tag no encontrado", null));
                }
                return Results.Ok(new BasicResponse<Tag>(true, "Detalles del Tag", tag));
            }
            catch (Exception)
            {

                return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
            }

        }
    }

    public class GetTagByIdResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}