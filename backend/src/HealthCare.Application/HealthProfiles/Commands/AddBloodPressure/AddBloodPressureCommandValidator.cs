using FluentValidation;

namespace HealthCare.Application.HealthProfiles.Commands.AddBloodPressure;

public class AddBloodPressureCommandValidator : AbstractValidator<AddBloodPressureCommand>
{
    public AddBloodPressureCommandValidator()
    {
        RuleFor(x => x.Systolic).InclusiveBetween(50, 300).WithMessage("Huyết áp tâm thu không hợp lệ.");
        RuleFor(x => x.Diastolic).InclusiveBetween(30, 200).WithMessage("Huyết áp tâm trương không hợp lệ.");
        RuleFor(x => x.MeasuredAt).LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Thời gian đo không được ở tương lai.");
        RuleFor(x => x.Arm).Must(v => v == "left" || v == "right").WithMessage("Arm phải là 'left' hoặc 'right'.");
        RuleFor(x => x.Position).Must(v => v == "sitting" || v == "standing" || v == "lying")
            .WithMessage("Position phải là 'sitting', 'standing' hoặc 'lying'.");
    }
}