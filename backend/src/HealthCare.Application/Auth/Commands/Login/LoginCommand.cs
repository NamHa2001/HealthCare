using HealthCare.Application.Common.Models;
using HealthCare.Application.Auth.DTOs;
using MediatR;

namespace HealthCare.Application.Auth.Commands.Login;

public record LoginCommand(string Email, string Password, string? IpAddress) : IRequest<Result<AuthResponseDto>>;