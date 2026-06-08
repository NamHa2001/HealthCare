using HealthCare.Application.Common.Models;
using HealthCare.Application.Features.Auth.DTOs;
using MediatR;

namespace HealthCare.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password, string? IpAddress) : IRequest<Result<AuthResponseDto>>;