using AutoMapper;

namespace LyricsApp.Application.Features.Groups;

public class GroupsMappingProfile : Profile
{
    public GroupsMappingProfile()
    {
        CreateMap<Domain.Entities.Group, GroupResponse>();

        CreateMap<Domain.Entities.Group, GroupDetailResponse>()
        .ForMember(dest => dest.Members, m => m.MapFrom(src => src.Members.Select(x => new GroupMember (x.User.Id, x.User.Name))));
    }
}

