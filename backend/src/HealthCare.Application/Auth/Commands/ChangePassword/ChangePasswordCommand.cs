using HealthCare.Application.Common.Models;
using MediatR;

namespace HealthCare.Application.Auth.Commands.ChangePassword;

public record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : IRequest<Result>;
