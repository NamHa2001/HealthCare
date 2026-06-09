using HealthCare.Application.HealthProfiles.DTOs;
using MediatR;

namespace HealthCare.Application.HealthProfiles.Queries.GetHealthProfile;

public record GetHealthProfileQuery : IRequest<HealthProfileDto>;
