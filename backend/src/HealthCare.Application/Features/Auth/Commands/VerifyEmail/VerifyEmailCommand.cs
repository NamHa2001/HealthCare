using HealthCare.Application.Common.Models;
using MediatR;

namespace HealthCare.Application.Features.Auth.Commands.VerifyEmail;

public record VerifyEmailCommand(string Token) : IRequest<Result>;
