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

namespace LyricsApp.Application.Features.Categories.Queries;

public class GetTagDetails : ICarterModule

{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/tags/{id}", (int id, IMediator mediator) =>
        {
            return mediator.Send(new GetTagsDetailQuery(id));
        })
        .WithName(nameof(GetTagDetails))
        .Produces(StatusCodes.Status200OK, typeof(BasicResponse<Tag>))
        .WithTags(nameof(Tag));
    }

    public record GetTagsDetailQuery(int Id) : IRequest<BasicResponse<Tag>>;

    public class GetTagDetailHandler : IRequestHandler<GetTagsDetailQuery, BasicResponse<Tag>>
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;

        public GetTagDetailHandler(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<BasicResponse<Tag>> Handle(GetTagsDetailQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var tag = await _context.Tags.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
                return new BasicResponse<Tag>(true, "Tag Detail", tag);
            }
            catch (Exception)
            {

                throw;
            }

        }
    }

    public class GetTagsResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}