using AutoMapper;
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

public class AddGroupMembers : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/groups/{id}/Members", (IMediator mediator, Guid id, [FromBody] IList<Guid> members) =>
        {
            return mediator.Send(new AddGroupMemberRequest(id, members));
        })
        .WithName(nameof(AddGroupMembers))
        .Produces(StatusCodes.Status200OK, typeof(BasicResponse<GroupDetailResponse>))
        .Produces(StatusCodes.Status404NotFound, typeof(BasicResponse<>))
        .Produces(StatusCodes.Status400BadRequest, typeof(BasicResponse<>))
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags(nameof(Group))
        .RequireAuthorization();
    }
}

public record AddGroupMemberRequest(Guid GroupId, IList<Guid> Members) : IRequest<IResult>;

public class AddGroupMembersHandler : IRequestHandler<AddGroupMemberRequest, IResult>
{
    private readonly ApiDbContext context;
    private readonly IMapper mapper;
    private readonly IHttpContextService httpContextService;

    public AddGroupMembersHandler(ApiDbContext context, IMapper mapper, IHttpContextService httpContextService)
    {
        this.context = context;
        this.mapper = mapper;
        this.httpContextService = httpContextService;
    }

    public async Task<IResult> Handle(AddGroupMemberRequest request, CancellationToken cancellationToken)
    {
        var group = await context.Groups
        .Include(i => i.Members)
        .FirstOrDefaultAsync(x => x.Id == request.GroupId, cancellationToken);

        if (group == null)
        {
            return Results.NotFound(new BasicResponse<GroupDetailResponse>(false, "Group not found", null));
        }

        if(group.AdminId != httpContextService.UserId)
        {
            return Results.BadRequest(new BasicResponse<GroupDetailResponse>(false, "Only admin can add members", null));
        }

        var NewMembers = new List<GroupAssignment>();

        foreach (var item in request.Members)
        {
            var newMember = new GroupAssignment(group.Id, item);
            if (!group.Members.Any(x => x.UserId == item))
                NewMembers.Add(newMember);
        }

        if (NewMembers.Count == 0)
        {
            return Results.BadRequest(new BasicResponse<GroupDetailResponse>(false, "The members are already added", null));
        }

        await context.GroupAssignments.AddRangeAsync(NewMembers, cancellationToken);

        context.SaveChanges();

        group = await context.Groups
                .Include(i => i.Members)
                .ThenInclude(i => i.User)
                .FirstOrDefaultAsync(x => x.Id == request.GroupId, cancellationToken);

        var groupDetails = mapper.Map<GroupDetailResponse>(group);

        return Results.Ok(new BasicResponse<GroupDetailResponse>(true, "Group details", groupDetails));
    }
}
