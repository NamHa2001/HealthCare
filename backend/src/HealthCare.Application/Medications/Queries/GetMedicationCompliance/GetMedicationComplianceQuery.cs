using HealthCare.Application.Medications.DTOs;
using MediatR;

namespace HealthCare.Application.Medications.Queries.GetMedicationCompliance;

public record GetMedicationComplianceQuery(int WeeksBack = 4) : IRequest<List<ComplianceReportDto>>;
