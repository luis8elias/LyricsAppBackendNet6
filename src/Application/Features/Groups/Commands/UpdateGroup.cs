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
        app.MapPut("api/groups/{id}", (IMediator mediator, Guid id, [FromBody] UpdateGroupName req) => mediator.Send(new UpdateGroupRequest(id, req.NewName)))
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

public record UpdateGroupName(string NewName);


public class UpdateGroupHandler : IRequestHandler<UpdateGroupRequest, IResult>
{
    private readonly ApiDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextService _httpContextService;

    public UpdateGroupHandler(ApiDbContext context, IMapper mapper, IHttpContextService httpContextService)
    {
        this._context = context;
        this._mapper = mapper;
        this._httpContextService = httpContextService;
    }

    public async Task<IResult> Handle(UpdateGroupRequest request, CancellationToken cancellationToken)
    {
        var group = await _context.Groups.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (group == null)
        {
            return Results.NotFound(new BasicResponse<GroupResponse>(false, "The group does not exits", null));
        }

        if (group.AdminId != _httpContextService.UserId)
        {
            return Results.BadRequest(new BasicResponse<GroupResponse>(false, "Only admin can add update the group", null));
        }

        group.UpdateName(request.Name);

        _context.Groups.Update(group);
        _context.SaveChanges();

        var result = _mapper.Map<GroupResponse>(group);

        return Results.Ok(new BasicResponse<GroupResponse>(true, "Group updated", result));
    }
}
