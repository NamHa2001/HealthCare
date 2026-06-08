using HealthCare.Application.Common.Models;
using MediatR;

namespace HealthCare.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(string Token, string NewPassword) : IRequest<Result>;
