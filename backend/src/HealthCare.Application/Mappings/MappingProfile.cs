using AutoMapper;
using HealthCare.Application.Auth.DTOs;
using HealthCare.Domain.Entities.Auth;
using HealthCare.Application.HealthProfiles.DTOs;
using HealthCare.Domain.Entities.HealthProfile;
namespace HealthCare.Application.Mappings;

public class MappingProfile : Profile
{


    public MappingProfile()
    {
        CreateMap<User, UserInfoDto>()
            .ForMember(d => d.Roles, opt => opt.Ignore());
        CreateMap<HealthProfile, HealthProfileDto>();
        CreateMap<HealthMeasurement, HealthMeasurementDto>();
        CreateMap<BloodPressureLog, BloodPressureLogDto>();
        CreateMap<HealthAlert, HealthAlertDto>();

    }
}