using AutoMapper;
using HealthCare.Application.Features.Auth.DTOs;
using HealthCare.Domain.Entities.Auth;

namespace HealthCare.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserInfoDto>()
            .ForMember(d => d.Roles, opt => opt.Ignore());
    }
}