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
        app.MapGet("api/tags/{tagId}", (IMediator mediator, Guid tagId) => mediator.Send(new GetTagByIdQuery(tagId)))
        .WithName(nameof(GetTagById))
        .Produces(StatusCodes.Status200OK, typeof(BasicResponse<Tag>))
        .Produces(StatusCodes.Status404NotFound, typeof(BasicResponse<>))
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags(nameof(Tag))
        .RequireAuthorization();
    }

    public record GetTagByIdQuery(Guid TagId) : IRequest<IResult>;

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
                var tag = await _context.Tags.FindAsync(new object?[] { request.TagId }, cancellationToken: cancellationToken);

                return tag is null
                    ? Results.NotFound(new BasicResponse<Tag?>(false, "Tag no encontrado", null))
                    : Results.Ok(new BasicResponse<Tag>(true, "Detalles del Tag", tag));
            }
            catch (Exception)
            {
                return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
            }

        }
    }

    public class GetTagByIdResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }
}