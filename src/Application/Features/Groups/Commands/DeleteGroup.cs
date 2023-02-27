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

namespace LyricsApp.Application.Features.Groups.Commands;

public class DeleteGroup : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/groups/{id}", (IMediator mediator, Guid id) =>
        {
            return mediator.Send(new DeleteGroupRequest(id));
        })
        .WithName(nameof(DeleteGroup))
        .Produces(StatusCodes.Status200OK, typeof(BasicResponse<GroupDetailResponse>))
        .Produces(StatusCodes.Status404NotFound, typeof(BasicResponse<>))
        .Produces(StatusCodes.Status400BadRequest, typeof(BasicResponse<>))
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags(nameof(Group))
        .RequireAuthorization();
    }
}

public record DeleteGroupRequest(Guid Id) : IRequest<IResult>;

public class DeleteGroupHandler : IRequestHandler<DeleteGroupRequest, IResult>
{
    private readonly ApiDbContext _context;
    private readonly IHttpContextService _httpContextService;

    public DeleteGroupHandler(ApiDbContext context, IHttpContextService httpContextService)
    {
        this._context = context;
        this._httpContextService = httpContextService;
    }

    public async Task<IResult> Handle(DeleteGroupRequest request, CancellationToken cancellationToken)
    {
        var group = await _context.Groups
        .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (group == null)
        {
            return Results.NotFound(new BasicResponse<GroupDetailResponse>(false, "The group does not exits", null));
        }

        if (group.AdminId != _httpContextService.UserId)
        {
            return Results.BadRequest(new BasicResponse<GroupDetailResponse>(false, "Only admin can add delete the group", null));
        }

        _context.Groups.Remove(group);
        _context.SaveChanges();

        return Results.Ok(new BasicResponse<GroupDetailResponse>(true, "The group was removed successfully", null));
    }
}
