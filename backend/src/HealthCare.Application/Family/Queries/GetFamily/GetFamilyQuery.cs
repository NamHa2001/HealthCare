using HealthCare.Application.Family.DTOs;
using MediatR;

namespace HealthCare.Application.Family.Queries.GetFamily;

public record GetFamilyQuery : IRequest<FamilyGroupDto?>;
