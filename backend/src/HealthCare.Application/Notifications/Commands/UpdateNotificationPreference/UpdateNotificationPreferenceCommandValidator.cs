using FluentValidation;

namespace HealthCare.Application.Notifications.Commands.UpdateNotificationPreference;

public class UpdateNotificationPreferenceCommandValidator
    : AbstractValidator<UpdateNotificationPreferenceCommand>
{
    private static readonly string[] AllowedTimezones =
    [
        "Asia/Ho_Chi_Minh", "Asia/Bangkok", "Asia/Singapore",
        "Asia/Tokyo", "Asia/Seoul", "UTC",
    ];

    public UpdateNotificationPreferenceCommandValidator()
    {
        RuleFor(x => x.Timezone)
            .NotEmpty()
            .Must(tz => AllowedTimezones.Contains(tz))
            .WithMessage("Timezone không hợp lệ.");

        RuleFor(x => x.QuietStartTime)
            .NotEmpty()
            .Matches(@"^\d{2}:\d{2}$").WithMessage("Giờ bắt đầu không đúng định dạng HH:mm.");

        RuleFor(x => x.QuietEndTime)
            .NotEmpty()
            .Matches(@"^\d{2}:\d{2}$").WithMessage("Giờ kết thúc không đúng định dạng HH:mm.");
    }
}
