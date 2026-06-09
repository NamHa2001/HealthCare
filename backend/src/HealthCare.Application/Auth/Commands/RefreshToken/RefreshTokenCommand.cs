using HealthCare.Application.Common.Models;
using HealthCare.Application.Auth.DTOs;
using MediatR;

namespace HealthCare.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string Token, string? IpAddress) : IRequest<Result<AuthResponseDto>>;
