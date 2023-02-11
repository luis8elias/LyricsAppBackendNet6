using AutoMapper;
using Carter;
using FluentValidation;
using LyricsApp.Application.Common.Models;
using LyricsApp.Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LyricsApp.Application.Features.Groups.Queries;

public class GetGroupById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/groups/{id}", (IMediator mediator, Guid id) =>
        {
            return mediator.Send(new GetGroupByIdRequest(id));
        })
        .WithName(nameof(GetGroupById))
        .Produces(StatusCodes.Status200OK, typeof(BasicResponse<GroupDetailResponse>))
        .Produces(StatusCodes.Status404NotFound, typeof(BasicResponse<>))
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags(nameof(Domain.Entities.Group))
        .RequireAuthorization();
    }
}

public record GetGroupByIdRequest(Guid Id) : IRequest<IResult>;

public class GetGroupByIdRequestValidator : AbstractValidator<GetGroupByIdRequest>
{
    public GetGroupByIdRequestValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
    }
}

public class GetGroupByIdHandler : IRequestHandler<GetGroupByIdRequest, IResult>
{
    private readonly ApiDbContext context;
    private readonly IMapper mapper;

    public GetGroupByIdHandler(ApiDbContext context, IMapper mapper)
    {
        this.context = context;
        this.mapper = mapper;
    }

    public async Task<IResult> Handle(GetGroupByIdRequest request, CancellationToken cancellationToken)
    {
        var group = await context.Groups
        .Include(i => i.Members)
        .ThenInclude(i => i.User)
        .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (group == null)
        {
            return Results.NotFound(new BasicResponse<GroupDetailResponse>(false, "Group details not found", null));
        }

        var groupDetails = mapper.Map<GroupDetailResponse>(group);

        return Results.Ok(new BasicResponse<GroupDetailResponse>(true, "Group details", groupDetails));

    }
}

