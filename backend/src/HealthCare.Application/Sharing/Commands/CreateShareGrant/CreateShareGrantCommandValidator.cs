using FluentValidation;
using HealthCare.Application.Sharing.Common;

namespace HealthCare.Application.Sharing.Commands.CreateShareGrant;

public class CreateShareGrantCommandValidator : AbstractValidator<CreateShareGrantCommand>
{
    private static readonly int[] AllowedTtlHours = [1, 24, 168]; // 1 giờ, 24 giờ, 7 ngày

    public CreateShareGrantCommandValidator()
    {
        RuleFor(x => x.HealthProfileId).NotEmpty();

        RuleFor(x => x.Scope)
            .NotEmpty().WithMessage("Phải chọn ít nhất một phạm vi dữ liệu chia sẻ.")
            .Must(s => s.All(ShareScopes.IsValid))
            .WithMessage($"Phạm vi hợp lệ: {string.Join(", ", ShareScopes.All)}");

        RuleFor(x => x.TtlHours)
            .Must(t => AllowedTtlHours.Contains(t))
            .WithMessage("Thời hạn hợp lệ: 1, 24 hoặc 168 giờ.");
    }
}
