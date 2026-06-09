namespace HealthCare.Application.Auth.DTOs;

public class UserInfoDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public bool IsEmailVerified { get; init; }
    public IEnumerable<string> Roles { get; init; } = [];
}