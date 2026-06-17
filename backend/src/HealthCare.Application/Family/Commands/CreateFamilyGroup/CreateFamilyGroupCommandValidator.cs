using FluentValidation;

namespace HealthCare.Application.Family.Commands.CreateFamilyGroup;

public class CreateFamilyGroupCommandValidator : AbstractValidator<CreateFamilyGroupCommand>
{
    public CreateFamilyGroupCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên nhóm không được để trống.")
            .MaximumLength(100).WithMessage("Tên nhóm tối đa 100 ký tự.");
    }
}
