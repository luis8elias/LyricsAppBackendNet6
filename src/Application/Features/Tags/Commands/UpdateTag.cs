using Carter;
using FluentValidation;
using LyricsApp.Application.Common.Filters;
using LyricsApp.Application.Common.Models;
using LyricsApp.Application.Domain.Entities;
using LyricsApp.Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LyricsApp.Application.Features.Tags.Commands
{
    public class UpdateTag : ICarterModule
    {

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/tags", async (UpdateTagCommand command,IMediator mediator) =>
            {
                return await mediator.Send(command);
            })
            .WithName(nameof(UpdateTag))
            .WithTags(nameof(Tag))
            .Produces(StatusCodes.Status404NotFound)
            .Produces(
                StatusCodes.Status200OK,
                typeof(BasicResponse<Tag?>)
            )
            .Produces(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .AddEndpointFilter<ValidationFilter<UpdateTagCommand>>();
            
        }
    }

    public class UpdateTagCommand : IRequest<IResult>
    {

        public int TagId { get; set; }
        public string NewName { get; set; } = string.Empty;
    }

    public class UpdateTagValidator : AbstractValidator<UpdateTagCommand>
    {
        public UpdateTagValidator()
        {
            RuleFor(r => r.TagId).NotEmpty().NotNull();
            RuleFor(r => r.NewName).NotEmpty().NotNull();
        }
    }

    public class UpdateTagHandler : IRequestHandler<UpdateTagCommand, IResult>
    {
        private readonly ApiDbContext _context;
        

        public UpdateTagHandler(ApiDbContext context )
        {
            _context = context;
        }

        public async Task<IResult> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
        {

            try
            {
                var tag = await _context.Tags.FindAsync(request.TagId);

                if (tag is null)
                {
                    return Results.NotFound(new BasicResponse<Tag?>(false, "Tag no encontrado", null));
                }

                tag.Name= request.NewName;

                await _context.SaveChangesAsync();

                return Results.Ok(new BasicResponse<Tag>(true, "Tag actualizado", tag));

            }
            catch (Exception)
            {
                return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }

}
