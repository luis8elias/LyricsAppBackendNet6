using Carter;
using Carter.ModelBinding;
using FluentValidation;
using LyricsApp.Application.Common.Filters;
using LyricsApp.Application.Common.Models;
using LyricsApp.Application.Domain.Entities;
using LyricsApp.Application.Features.Categories.Queries;
using LyricsApp.Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;


namespace LyricsApp.Application.Features.Tags.Commands
{
    public class CreateTag : ICarterModule

    {

        public void AddRoutes(IEndpointRouteBuilder app)
        {

            app.MapPost("api/tags", async (CreateTagCommand command, IMediator mediator) =>
            {
                return await mediator.Send(command);
            })
            .WithName(nameof(CreateTag))
            .WithTags(nameof(Tag))
            .ProducesValidationProblem()
            .Produces(
                StatusCodes.Status201Created,
                typeof(BasicResponse<int>)
            )
            .AddEndpointFilter<ValidationFilter<CreateTagCommand>>();
        }
    }

    public class CreateTagCommand : IRequest<IResult>
    {
        public string Name { get; set; } = string.Empty;

    }

    public class CreateTagValidator : AbstractValidator<CreateTagCommand>
    {
        public CreateTagValidator()
        {
            RuleFor(r => r.Name).NotEmpty().NotNull();
        }
    }

    public class CreateTagHandler : IRequestHandler<CreateTagCommand, IResult>
    {
        private readonly ApiDbContext _context;
        private readonly IValidator<CreateTagCommand> _validator;

        public CreateTagHandler(ApiDbContext context, IValidator<CreateTagCommand> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<IResult> Handle(CreateTagCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var newTag = new Tag(0, request.Name);
                
                _context.Tags.Add(newTag);

                await _context.SaveChangesAsync(cancellationToken);
                
                return Results.CreatedAtRoute(nameof(GetTagDetails), new { newTag.Id },
                    new BasicResponse<Tag>(true, "Tag creado correctamente", newTag));

            }
            catch (Exception)
            {
                return Results.BadRequest(new BasicResponse<int?>(false, "Tag no creado", null));

            }
        }
    }
}
