using HealthCare.Application.Vaccines.DTOs;
using MediatR;

namespace HealthCare.Application.Vaccines.Commands.CreateVaccineRecord;

public record CreateVaccineRecordCommand(
    string VaccineName,
    int DoseNumber,
    DateOnly InjectionDate,
    Guid? VaccineCatalogId,
    string? Facility,
    string? LotNumber,
    string? AdministeredBy,
    string? Reaction,
    Guid? DocumentId) : IRequest<VaccineRecordDto>;
