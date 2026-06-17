using HealthCare.Application.Vaccines.DTOs;
using MediatR;

namespace HealthCare.Application.Vaccines.Queries.GetVaccineRecords;

public record GetVaccineRecordsQuery : IRequest<IReadOnlyList<VaccineRecordDto>>;
