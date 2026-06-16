using AutoMapper;
using HealthCare.Application.Auth.DTOs;
using HealthCare.Application.HealthProfiles.DTOs;
using HealthCare.Application.MedicalHistory.DTOs;
using HealthCare.Application.Medications.DTOs;
using HealthCare.Domain.Entities.Auth;
using HealthCare.Domain.Entities.HealthProfile;
using HealthCare.Domain.Entities.MedicalHistory;
using HealthCare.Domain.Entities.Medications;

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

        CreateMap<MedicalDocument, MedicalDocumentDto>();
        CreateMap<MedicalVisit, MedicalVisitDto>()
            .ForMember(d => d.Documents, opt => opt.MapFrom(s => s.Documents));
        CreateMap<MedicalVisit, MedicalVisitListDto>()
            .ForMember(d => d.DocumentCount, opt => opt.MapFrom(s => s.Documents.Count));

        CreateMap<Medication, MedicationDto>()
            .ForMember(d => d.Schedules, opt => opt.MapFrom(s => s.Schedules));
        CreateMap<MedicationSchedule, MedicationScheduleDto>();
        CreateMap<MedicationLog, MedicationLogDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));
    }
}