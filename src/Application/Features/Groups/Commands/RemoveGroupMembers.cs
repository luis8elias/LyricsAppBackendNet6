using Carter;
using LyricsApp.Application.Common.Models;
using LyricsApp.Application.Domain.Entities;
using LyricsApp.Application.Domain.Interfaces;
using LyricsApp.Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LyricsApp.Application.Features.Groups.Commands;

public class RemoveGroupMembers : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        _ = app.MapDelete("api/groups/{id}/Members", (IMediator mediator, Guid id, [FromBody] IList<Guid> members) => mediator.Send(new RemoveGroupMembersRequest(id, members)))
        .WithName(nameof(RemoveGroupMembers))
        .Produces(StatusCodes.Status200OK, typeof(BasicResponse<GroupDetailResponse>))
        .Produces(StatusCodes.Status404NotFound, typeof(BasicResponse<>))
        .Produces(StatusCodes.Status400BadRequest, typeof(BasicResponse<>))
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags(nameof(Group))
        .RequireAuthorization();
    }
}
public record GroupMember(Guid UserId);

public record RemoveGroupMembersRequest(Guid GroupId, IList<Guid> Members) : IRequest<IResult>;

public class RemoveGroupMembersHandler : IRequestHandler<RemoveGroupMembersRequest, IResult>
{
    private readonly ApiDbContext _context;
    private readonly IHttpContextService _httpContextService;

    public RemoveGroupMembersHandler(ApiDbContext context, IHttpContextService httpContextService)
    {
        _context = context;
        _httpContextService = httpContextService;
    }

    public async Task<IResult> Handle(RemoveGroupMembersRequest request, CancellationToken cancellationToken)
    {
        var group = await _context.Groups
        .Include(i => i.Members)
        .FirstOrDefaultAsync(x => x.Id == request.GroupId, cancellationToken);

        if (group == null)
        {
            return Results.NotFound(new BasicResponse<GroupDetailResponse>(false, "Group not found", null));
        }

        if (group.AdminId != _httpContextService.UserId)
        {
            return Results.BadRequest(new BasicResponse<GroupDetailResponse>(false, "Only admin can remove members", null));
        }

        var removeMembers = _context.GroupAssignments.Where(x => x.GroupId == request.GroupId && request.Members.Contains(x.UserId)).ToList();

        _context.GroupAssignments.RemoveRange(removeMembers);

        await _context.SaveChangesAsync(cancellationToken);

        return Results.Ok(new BasicResponse<GroupDetailResponse>(true, "The Members were removed", null));
    }
}
