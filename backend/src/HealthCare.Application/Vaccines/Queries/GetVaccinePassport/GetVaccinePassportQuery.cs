using MediatR;

namespace HealthCare.Application.Vaccines.Queries.GetVaccinePassport;

public record GetVaccinePassportQuery : IRequest<byte[]>;
