using HealthCare.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace HealthCare.Infrastructure.Services.Auth;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;

    public SmtpEmailService(IConfiguration config) => _config = config;

    public async Task SendEmailVerificationAsync(string toEmail, string name, string token, CancellationToken ct = default)
    {
        var link = $"{_config["App:FrontendUrl"]}/auth/verify-email?token={token}";
        await SendAsync(toEmail, "Xác nhận email — Health+",
            $"<p>Xin chào {name},</p><p>Nhấn <a href='{link}'>vào đây</a> để xác nhận email. Link có hiệu lực 24 giờ.</p>",
            ct);
    }

    public async Task SendPasswordResetAsync(string toEmail, string name, string token, CancellationToken ct = default)
    {
        var link = $"{_config["App:FrontendUrl"]}/auth/reset-password?token={token}";
        await SendAsync(toEmail, "Đặt lại mật khẩu — Health+",
            $"<p>Xin chào {name},</p><p>Nhấn <a href='{link}'>vào đây</a> để đặt lại mật khẩu. Link có hiệu lực 2 giờ.</p>",
            ct);
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            _config["Email:FromName"] ?? "Health+",
            _config["Email:FromAddress"] ?? "no-reply@healthplus.vn"));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(
            _config["Email:Host"],
            int.Parse(_config["Email:Port"] ?? "587"),
            MailKit.Security.SecureSocketOptions.Auto, ct);

        var user = _config["Email:Username"];
        var pass = _config["Email:Password"];
        if (!string.IsNullOrEmpty(user))
            await smtp.AuthenticateAsync(user, pass, ct);

        await smtp.SendAsync(message, ct);
        await smtp.DisconnectAsync(true, ct);
    }
}