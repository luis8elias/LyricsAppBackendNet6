using Carter;
using Carter.ModelBinding;
using FluentValidation;
using LyricsApp.Application.Common.Models;
using LyricsApp.Application.Domain.Entities;

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
           
            app.MapPost("api/tags", async (HttpRequest req, IMediator mediator, CreateTagCommand command) =>
            {
                return await mediator.Send(command);
            })
              
            .WithName(nameof(CreateTag))
            .WithTags(nameof(Tag))
            .ProducesValidationProblem()
            .Produces(
                StatusCodes.Status201Created,
                typeof(BasicResponse<int>)
             );
           
        }
    }

    public class CreateTagCommand : IRequest<BasicResponse<int?>>
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

    public class CreateTagHandler : IRequestHandler<CreateTagCommand, BasicResponse<int?>>
    {
        private readonly ApiDbContext _context;
        private readonly IValidator<CreateTagCommand> _validator;

        public CreateTagHandler(ApiDbContext context, IValidator<CreateTagCommand> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<BasicResponse<int?>> Handle(CreateTagCommand request, CancellationToken cancellationToken)
        {
            var result = _validator.Validate(request);
            if (!result.IsValid)
            {
                var errs = result.GetValidationProblems();
                return new BasicResponse<int?>(false, "Tag no creado", null);
               
            }

            try
            {
                var newTag = new Tag(0, request.Name);
                _context.Tags.Add(newTag);
                await _context.SaveChangesAsync();
                return new BasicResponse<int?>(true, "Tag creado correctamente", newTag.Id);

            }
            catch (Exception)
            {
                return new BasicResponse<int?>(false, "Tag no creado", null);
                
            }



            

        }
    }
}
