using HealthCare.Application.Analytics.DTOs;
using MediatR;

namespace HealthCare.Application.Analytics.Queries.GetMedicationCompliance;

public record GetMedicationComplianceQuery(int Weeks = 8) : IRequest<IReadOnlyList<ComplianceWeekDto>>;
