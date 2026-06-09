using HealthCare.Application.Common.Models;
using MediatR;

namespace HealthCare.Application.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest<Result>;
