using AutoMapper;
using LyricsApp.Application.Features.Genres;

namespace LyricsApp.Application.Features.Groups;

public class GenreMappingProfile : Profile
{
    public GenreMappingProfile()
    {
        CreateMap<Domain.Entities.Genre, GenreResponse>();
    }
        
}