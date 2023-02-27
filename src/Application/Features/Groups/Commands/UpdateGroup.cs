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

public class UpdateGroup : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/groups/{id}", (IMediator mediator, Guid id, [FromBody] UpdateGroupName req) =>
        {
            return mediator.Send(new UpdateGroupRequest(id, req.newName));
        })
        .WithName(nameof(UpdateGroup))
        .Produces(StatusCodes.Status200OK, typeof(BasicResponse<GroupResponse>))
        .Produces(StatusCodes.Status404NotFound, typeof(BasicResponse<>))
        .Produces(StatusCodes.Status400BadRequest, typeof(BasicResponse<>))
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags(nameof(Group))
        .RequireAuthorization();
    }
}

public record UpdateGroupRequest(Guid Id, string Name) : IRequest<IResult>;

public record UpdateGroupName(string newName);


public class UpdateGroupHandler : IRequestHandler<UpdateGroupRequest, IResult>
{
    private readonly ApiDbContext context;
    private readonly IMapper mapper;
    private readonly IHttpContextService httpContextService;

    public UpdateGroupHandler(ApiDbContext context, IMapper mapper, IHttpContextService httpContextService)
    {
        this.context = context;
        this.mapper = mapper;
        this.httpContextService = httpContextService;
    }

    public async Task<IResult> Handle(UpdateGroupRequest request, CancellationToken cancellationToken)
    {
        var group = await context.Groups.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (group == null)
        {
            return Results.NotFound(new BasicResponse<GroupResponse>(false, "The group does not exits", null));
        }

        if (group.AdminId != httpContextService.UserId)
        {
            return Results.BadRequest(new BasicResponse<GroupResponse>(false, "Only admin can add update the group", null));
        }

        group.UpdateName(request.Name);

        context.Groups.Update(group);
        context.SaveChanges();

        var result = mapper.Map<GroupResponse>(group);

        return Results.Ok(new BasicResponse<GroupResponse>(true, "Group updated", result));
    }
}
