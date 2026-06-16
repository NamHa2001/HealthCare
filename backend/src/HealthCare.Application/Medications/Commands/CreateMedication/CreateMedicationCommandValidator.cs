using FluentValidation;

namespace HealthCare.Application.Medications.Commands.CreateMedication;

public class CreateMedicationCommandValidator : AbstractValidator<CreateMedicationCommand>
{
    public CreateMedicationCommandValidator()
    {
        RuleFor(x => x.DrugName)
            .NotEmpty().WithMessage("Tên thuốc không được để trống.")
            .MaximumLength(255).WithMessage("Tên thuốc tối đa 255 ký tự.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Ngày bắt đầu không được để trống.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now))
            .WithMessage("Ngày bắt đầu không được ở tương lai.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("Ngày kết thúc phải sau hoặc bằng ngày bắt đầu.");

        RuleFor(x => x.ConfidenceScore)
            .InclusiveBetween(0m, 1m)
            .When(x => x.ConfidenceScore.HasValue)
            .WithMessage("Điểm tin cậy phải từ 0 đến 1.");
    }
}
