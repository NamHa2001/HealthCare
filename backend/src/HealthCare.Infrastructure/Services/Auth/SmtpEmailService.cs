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

    public async Task SendReminderAsync(string toEmail, string title, string body, CancellationToken ct = default)
    {
        await SendAsync(toEmail, $"[Health+] {title}",
            $"<p>{body}</p><p><small>Được gửi bởi Health+ Reminder Engine</small></p>",
            ct);
    }

    public async Task SendAccountLockedAsync(string toEmail, string name, CancellationToken ct = default)
    {
        await SendAsync(toEmail, "Tài khoản bị khóa tạm thời — Health+",
            $"<p>Xin chào {name},</p>" +
            $"<p>Tài khoản của bạn đã bị <strong>khóa 24 giờ</strong> do đăng nhập sai quá 10 lần liên tiếp.</p>" +
            $"<p>Nếu không phải bạn thực hiện, hãy đổi mật khẩu ngay sau khi mở khóa.</p>" +
            $"<p><small>Health+ Security Team</small></p>",
            ct);
    }

    public async Task SendFamilyInviteAsync(string toEmail, string inviterName, string groupName, string token, CancellationToken ct = default)
    {
        var link = $"{_config["App:FrontendUrl"]}/family/accept-invite?token={token}";
        await SendAsync(toEmail, $"{inviterName} mời bạn vào nhóm gia đình — Health+",
            $"<p>Xin chào,</p>" +
            $"<p><strong>{inviterName}</strong> đã mời bạn tham gia nhóm gia đình <strong>{groupName}</strong> trên Health+.</p>" +
            $"<p><a href='{link}'>Chấp nhận lời mời</a> (có hiệu lực trong 7 ngày).</p>" +
            $"<p>Nếu bạn chưa có tài khoản, hãy đăng ký tại <a href='{_config["App:FrontendUrl"]}/auth/register'>đây</a> trước.</p>",
            ct);
    }

    public async Task SendDoctorApprovedAsync(string toEmail, string name, CancellationToken ct = default)
    {
        var link = $"{_config["App:FrontendUrl"]}/doctor-registration";
        await SendAsync(toEmail, "Hồ sơ bác sĩ đã được duyệt — Health+",
            $"<p>Xin chào BS. {name},</p>" +
            $"<p>Hồ sơ bác sĩ của bạn đã được <strong>xác minh và phê duyệt</strong>. " +
            $"Đăng nhập lại để kích hoạt quyền bác sĩ.</p>" +
            $"<p><a href='{link}'>Xem trạng thái hồ sơ</a></p>",
            ct);
    }

    public async Task SendDoctorRejectedAsync(string toEmail, string name, string reason, CancellationToken ct = default)
    {
        var link = $"{_config["App:FrontendUrl"]}/doctor-registration";
        await SendAsync(toEmail, "Hồ sơ bác sĩ chưa được duyệt — Health+",
            $"<p>Xin chào {name},</p>" +
            $"<p>Hồ sơ bác sĩ của bạn chưa được phê duyệt. Lý do:</p>" +
            $"<blockquote>{reason}</blockquote>" +
            $"<p>Bạn có thể <a href='{link}'>cập nhật và nộp lại hồ sơ</a>.</p>",
            ct);
    }

    public async Task SendDoctorLinkInviteAsync(string toEmail, string name, string inviterDescription, CancellationToken ct = default)
    {
        var link = $"{_config["App:FrontendUrl"]}/doctor-links";
        await SendAsync(toEmail, "Lời mời liên kết theo dõi sức khỏe — Health+",
            $"<p>Xin chào {name},</p>" +
            $"<p><strong>{inviterDescription}</strong> muốn liên kết theo dõi sức khỏe với bạn trên Health+.</p>" +
            $"<p><a href='{link}'>Xem và phản hồi lời mời</a></p>" +
            $"<p><small>Bạn toàn quyền chấp nhận, từ chối, hoặc thu hồi liên kết bất cứ lúc nào.</small></p>",
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