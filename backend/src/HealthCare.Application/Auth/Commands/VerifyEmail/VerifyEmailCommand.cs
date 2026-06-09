using HealthCare.Application.Common.Models;
using MediatR;

namespace HealthCare.Application.Auth.Commands.VerifyEmail;

public record VerifyEmailCommand(string Token) : IRequest<Result>;
