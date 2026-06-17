namespace HealthCare.Application.Admin.DTOs;

public class AdminUserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public bool IsEmailVerified { get; init; }
    public bool IsActive { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public IEnumerable<string> Roles { get; init; } = [];
}
