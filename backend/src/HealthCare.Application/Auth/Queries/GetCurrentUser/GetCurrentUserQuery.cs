using HealthCare.Application.Common.Models;
using HealthCare.Application.Auth.DTOs;
using MediatR;

namespace HealthCare.Application.Auth.Queries.GetCurrentUser;

public record GetCurrentUserQuery(Guid UserId) : IRequest<Result<UserInfoDto>>;
