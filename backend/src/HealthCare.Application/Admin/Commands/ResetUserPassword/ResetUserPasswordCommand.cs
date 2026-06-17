using MediatR;

namespace HealthCare.Application.Admin.Commands.ResetUserPassword;

/// <summary>SRS §10.1 — Admin reset mật khẩu thủ công.</summary>
public record ResetUserPasswordCommand(Guid UserId, string NewPassword) : IRequest;
