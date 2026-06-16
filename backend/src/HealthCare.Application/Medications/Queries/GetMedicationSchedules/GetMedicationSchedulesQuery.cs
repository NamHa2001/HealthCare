using HealthCare.Application.Medications.DTOs;
using MediatR;

namespace HealthCare.Application.Medications.Queries.GetMedicationSchedules;

/// <summary>
/// Trả danh sách log uống thuốc trong ngày (today's schedule).
/// </summary>
public record GetMedicationSchedulesQuery(DateOnly? Date = null) : IRequest<List<MedicationLogDto>>;
