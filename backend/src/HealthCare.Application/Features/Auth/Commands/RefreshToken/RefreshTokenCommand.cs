using HealthCare.Application.Common.Models;
using HealthCare.Application.Features.Auth.DTOs;
using MediatR;

namespace HealthCare.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string Token, string? IpAddress) : IRequest<Result<AuthResponseDto>>;
