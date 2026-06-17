
using HealthCare.Domain.Common;
using HealthCare.Domain.Enums;

namespace HealthCare.Domain.Entities.Auth;

public class User : AuditableEntity
{
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string? PhoneNumber { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? LastLoginAt { get; private set; }
    public int FailedLoginCount { get; private set; }
    public DateTime? LockedUntil { get; private set; }

    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private User() { }

    public static User Create(string email, string passwordHash, string firstName, string lastName)
    {
        return new User
        {
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName
        };
    }

    public void VerifyEmail() => IsEmailVerified = true;
    public void RecordLogin() => LastLoginAt = DateTime.UtcNow;
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
    public void UpdatePassword(string newHash) => PasswordHash = newHash;
    public bool IsLockedOut() =>
    LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;

    public void RecordFailedLogin()
    {
        FailedLoginCount++;
        if (FailedLoginCount >= 10)
            LockedUntil = DateTime.UtcNow.AddHours(24);
        else if (FailedLoginCount >= 5)
            LockedUntil = DateTime.UtcNow.AddMinutes(15);
    }

    public void ResetFailedLogin()
    {
        FailedLoginCount = 0;
        LockedUntil = null;
    }

    public void UpdateProfile(string firstName, string lastName, string? phoneNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        UpdatedAt = DateTime.UtcNow;
    }
}