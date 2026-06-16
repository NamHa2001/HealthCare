using FluentValidation;

namespace HealthCare.Application.MedicalHistory.Commands.CreateMedicalVisit;

public class CreateMedicalVisitCommandValidator : AbstractValidator<CreateMedicalVisitCommand>
{
    public CreateMedicalVisitCommandValidator()
    {
        RuleFor(x => x.VisitDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Ngày khám không được ở tương lai.");

        RuleFor(x => x.FacilityName)
            .NotEmpty().WithMessage("Tên cơ sở y tế là bắt buộc.")
            .MaximumLength(255);

        RuleFor(x => x.ChiefComplaint)
            .NotEmpty().WithMessage("Lý do khám là bắt buộc.");

        RuleFor(x => x.Diagnosis)
            .NotEmpty().WithMessage("Chẩn đoán là bắt buộc.");

        When(x => x.FollowUpDate.HasValue, () =>
            RuleFor(x => x.FollowUpDate)
                .GreaterThanOrEqualTo(x => x.VisitDate)
                .WithMessage("Ngày tái khám phải sau ngày khám."));

        When(x => x.Cost.HasValue, () =>
            RuleFor(x => x.Cost)
                .GreaterThanOrEqualTo(0).WithMessage("Chi phí không được âm."));
    }
}
