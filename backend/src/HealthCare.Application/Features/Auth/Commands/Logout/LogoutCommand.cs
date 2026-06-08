using HealthCare.Application.Common.Models;
using MediatR;

namespace HealthCare.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest<Result>;
