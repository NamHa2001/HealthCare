using HealthCare.Application.Medications.DTOs;
using MediatR;

namespace HealthCare.Application.Medications.Queries.GetMedications;

public record GetMedicationsQuery(bool ActiveOnly = false) : IRequest<List<MedicationDto>>;
