using HealthCare.Application.Common.Models;
using MediatR;

namespace HealthCare.Application.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber
) : IRequest<Result<Guid>>;