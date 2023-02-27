using Carter;
using FluentValidation;
using LyricsApp.Application.Domain.Interfaces;
using LyricsApp.Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using AutoMapper;
using LyricsApp.Application.Common.Models;
using LyricsApp.Application.Common.Filters;
using Microsoft.AspNetCore.Builder;

namespace LyricsApp.Application.Features.Groups.Commands;

public class CreateGroup : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/groups", async (CreateGroupRequest command, IMediator mediator) => await mediator.Send(command))
           .WithName(nameof(CreateGroup))
           .WithTags(nameof(Domain.Entities.Group))
           .ProducesValidationProblem()
           .Produces(
               StatusCodes.Status201Created,
               typeof(BasicResponse<int>)
           )
           .AddEndpointFilter<ValidationFilter<CreateGroupRequest>>()
           .RequireAuthorization();
    }
}

public record CreateGroupRequest(string Name) : IRequest<IResult>;

public class CreateGroupRequestValidator : AbstractValidator<CreateGroupRequest>
{
    public CreateGroupRequestValidator()
    {
        RuleFor(r => r.Name).NotEmpty().NotNull();
    }
}

public class CreateGroupHandler : IRequestHandler<CreateGroupRequest, IResult>
{
    private readonly ApiDbContext _context;
    private readonly IHttpContextService _httpContextService;
    private readonly IMapper _mapper;

    public CreateGroupHandler(ApiDbContext context, IHttpContextService httpContextService, IMapper mapper)
    {
        _context = context;
        _httpContextService = httpContextService;
        _mapper = mapper;
    }

    public async Task<IResult> Handle(CreateGroupRequest request, CancellationToken cancellationToken)
    {
        var group = new Domain.Entities.Group(request.Name, _httpContextService.UserId);
        _context.Groups.Add(group);
        await _context.SaveChangesAsync(cancellationToken);
        var result = _mapper.Map<GroupResponse>(group);

        return Results.CreatedAtRoute(nameof(CreateGroup), new { group.Id },
                  new BasicResponse<GroupResponse>(true, "Group successfully created", result));
    }
}
