using AutoMapper;

using LyricsApp.Application.Domain.Entities;

namespace LyricsApp.Application.Features.Songs.Mapping
{
    public class SongsMappingProfile : Profile
    {
        public SongsMappingProfile()
        {
            CreateMap<Song, SongDto>()
            .ForMember(x => x.Genre, src => src.MapFrom(m => m.Genre.Name))
            .ForMember(x => x.Tags, src => src.MapFrom(m => m.Tags.Select(s => s.Name)));
        }
    }
}