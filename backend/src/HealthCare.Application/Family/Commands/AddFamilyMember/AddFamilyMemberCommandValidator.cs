using FluentValidation;

namespace HealthCare.Application.Family.Commands.AddFamilyMember;

public class AddFamilyMemberCommandValidator : AbstractValidator<AddFamilyMemberCommand>
{
    private static readonly string[] AllowedGenders = ["male", "female", "other"];
    private static readonly string[] AllowedRelationships = ["self", "spouse", "child", "parent", "sibling"];

    public AddFamilyMemberCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ tên không được để trống.")
            .MaximumLength(255).WithMessage("Họ tên tối đa 255 ký tự.");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Ngày sinh không được để trống.")
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Ngày sinh không được trong tương lai.");

        RuleFor(x => x.Gender)
            .NotEmpty()
            .Must(g => AllowedGenders.Contains(g))
            .WithMessage("Giới tính phải là: male, female, other.");

        RuleFor(x => x.Relationship)
            .Must(r => r is null || AllowedRelationships.Contains(r))
            .WithMessage("Mối quan hệ phải là: self, spouse, child, parent, sibling.");
    }
}
