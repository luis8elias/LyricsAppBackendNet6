using AutoMapper;
using AutoMapper.QueryableExtensions;
using Carter;
using LyricsApp.Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using LyricsApp.Application.Domain.Entities;
using LyricsApp.Application.Common.Models;

namespace LyricsApp.Application.Features.Tags.Queries;

public class GetTags : ICarterModule

{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/tags", (IMediator mediator) =>
        {
            return mediator.Send(new GetTagsQuery());
        })
        .WithName(nameof(GetTags))
        .Produces(StatusCodes.Status200OK,typeof(BasicResponse<List<GetTagsResponse>>))
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags(nameof(Tag))
        .RequireAuthorization();
    }

    public class GetTagsQuery : IRequest<IResult>
    {

    }

    public class GetTagsHandler : IRequestHandler<GetTagsQuery, IResult>
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;

        public GetTagsHandler(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IResult> Handle(GetTagsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var tagsList = await _context.Tags.ProjectTo<GetTagsResponse>(_mapper.ConfigurationProvider).ToListAsync();
                return Results.Ok(new BasicResponse<List<GetTagsResponse>>(true, "Listado de Tags", tagsList));
            }
            catch (Exception)
            {

                return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
            }

        }
    }

    public class GetTagsResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }

    public class GetCategoriesMappingProfile : Profile
    {
        public GetCategoriesMappingProfile() => CreateMap<Tag, GetTagsResponse>();
    }
}