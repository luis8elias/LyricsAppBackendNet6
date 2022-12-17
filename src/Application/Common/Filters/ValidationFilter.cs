using System.Net;
using Carter.ModelBinding;
using FluentValidation;
using LyricsApp.Application.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LyricsApp.Application.Common.Filters;

public class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        T? argToValidate = context.GetArgument<T>(0);
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();

        if (validator is not null)
        {
            var validationResult =  validator.Validate(argToValidate!);
            if (!validationResult.IsValid)
            {
                var validation = new CustomValidationException(validationResult.Errors);
                // return Results.ValidationProblem(validation.Errors, statusCode: (int)HttpStatusCode.BadRequest);

                return Results.BadRequest(validation.Result);
            }
        }

        // Otherwise invoke the next filter in the pipeline
        return await next.Invoke(context);
    }
}