﻿using AutoMapper;
using Carter;
using FluentValidation;
using LyricsApp.Application.Common.Models;
using LyricsApp.Application.Domain.Entities;
using LyricsApp.Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LyricsApp.Application.Features.Genres.Commands
{
    public class UpdateGenre : ICarterModule
    {

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/genre/{genreId}", (IMediator mediator, Guid genreId, [FromBody] NamedBodyParameter request) => mediator.Send(new UpdateGenreCommand(genreId, request.NewName)))
            .WithName(nameof(UpdateGenre))
            .WithTags(nameof(Genre))
            .Produces(StatusCodes.Status404NotFound)
            .Produces(
                StatusCodes.Status200OK,
                typeof(BasicResponse<GenreResponse?>)
            )
            .Produces(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem();

        }
    }

    public record NamedBodyParameter(string NewName);
    public record UpdateGenreCommand(Guid GenreId, string NewName) : IRequest<IResult>;

    public class UpdateGenreHandler : IRequestHandler<UpdateGenreCommand, IResult>
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;


        public UpdateGenreHandler(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IResult> Handle(UpdateGenreCommand request, CancellationToken cancellationToken)
        {

            try
            {
                var genre = await _context.Genres.FirstOrDefaultAsync(genre => genre.Id == request.GenreId, cancellationToken: cancellationToken);

                if (genre is null)
                {
                    return Results.NotFound(new BasicResponse<GenreResponse?>(false, "Género no encontrado", null));
                }
                genre.UpdateName(request.NewName);
                await _context.SaveChangesAsync(cancellationToken);
                var genreDetails = _mapper.Map<GenreResponse>(genre);

                return Results.Ok(new BasicResponse<GenreResponse>(true, "Género actualizado", genreDetails));

            }
            catch (Exception)
            {
                return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }

}