using HealthCare.Application.Auth.DTOs;
using HealthCare.Application.Common.Models;
using MediatR;

namespace HealthCare.Application.Auth.Commands.UpdateProfile;

public record UpdateProfileCommand(
    string FirstName,
    string LastName,
    string? PhoneNumber) : IRequest<Result<UserInfoDto>>;
