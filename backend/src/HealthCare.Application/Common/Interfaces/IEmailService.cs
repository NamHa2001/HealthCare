namespace HealthCare.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string toEmail, string name, string token, CancellationToken ct = default);
    Task SendPasswordResetAsync(string toEmail, string name, string token, CancellationToken ct = default);
    Task SendReminderAsync(string toEmail, string title, string body, CancellationToken ct = default);
    Task SendAccountLockedAsync(string toEmail, string name, CancellationToken ct = default);
    Task SendSecurityAlertAsync(string toEmail, string name, string reason, CancellationToken ct = default);
    Task SendFamilyInviteAsync(string toEmail, string inviterName, string groupName, string token, CancellationToken ct = default);
    Task SendDoctorApprovedAsync(string toEmail, string name, CancellationToken ct = default);
    Task SendDoctorRejectedAsync(string toEmail, string name, string reason, CancellationToken ct = default);
    Task SendDoctorLinkInviteAsync(string toEmail, string name, string inviterDescription, CancellationToken ct = default);
    Task SendDoctorAlertAsync(string toEmail, string doctorName, string patientName, string alertMessage, CancellationToken ct = default);
    Task SendDoctorDigestAsync(string toEmail, string doctorName, IReadOnlyList<string> lines, CancellationToken ct = default);
}