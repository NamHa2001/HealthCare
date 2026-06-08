using HealthCare.Application.Common.Models;
using HealthCare.Application.Features.Auth.DTOs;
using MediatR;

namespace HealthCare.Application.Features.Auth.Queries.GetCurrentUser;

public record GetCurrentUserQuery(Guid UserId) : IRequest<Result<UserInfoDto>>;
