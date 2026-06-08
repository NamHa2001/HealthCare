namespace HealthCare.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Email { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }
}