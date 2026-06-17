using HealthCare.Application.Vaccines.DTOs;
using MediatR;

namespace HealthCare.Application.Vaccines.Commands.UpdateVaccineRecord;

public record UpdateVaccineRecordCommand(
    Guid Id,
    string VaccineName,
    int DoseNumber,
    DateOnly InjectionDate,
    DateOnly? NextDueDate,
    string? Facility,
    string? LotNumber,
    string? AdministeredBy,
    string? Reaction) : IRequest<VaccineRecordDto>;
