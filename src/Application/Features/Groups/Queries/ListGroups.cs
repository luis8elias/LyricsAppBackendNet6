using AutoMapper;
using AutoMapper.QueryableExtensions;
using Carter;
using LyricsApp.Application.Common.Models;
using LyricsApp.Application.Domain.Interfaces;
using LyricsApp.Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LyricsApp.Application.Features.Groups.Queries;

public class ListGroups : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/groups", (IMediator mediator) =>
        {
            return mediator.Send(new ListGroupsRequest());
        })
        .WithName(nameof(ListGroups))
        .Produces(StatusCodes.Status200OK, typeof(BasicResponse<IEnumerable<GroupResponse>>))
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags(nameof(Domain.Entities.Group))
        .RequireAuthorization();
    }
}

public record ListGroupsRequest() : IRequest<IResult>;

public class ListGroupsHandler : IRequestHandler<ListGroupsRequest, IResult>
{
    private readonly ApiDbContext context;
    private readonly IHttpContextService httpContextService;
    private readonly IMapper mapper;

    public ListGroupsHandler(ApiDbContext context, IHttpContextService httpContextService, IMapper mapper)
    {
        this.context = context;
        this.httpContextService = httpContextService;
        this.mapper = mapper;
    }

    public async Task<IResult> Handle(ListGroupsRequest request, CancellationToken cancellationToken)
    {
        var userId = httpContextService.UserId;
        var groups = await context.Groups.Where(x => x.AdminId == userId || x.Members.Any(m => m.UserId == userId))
            .ProjectTo<GroupResponse>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken: cancellationToken);

        return Results.Ok(new BasicResponse<IEnumerable<GroupResponse>>(true, "Groups List", groups));
    }
}
