using FluentValidation;

namespace HealthCare.Application.HealthProfiles.Commands.AddMeasurement;

public class AddMeasurementCommandValidator : AbstractValidator<AddMeasurementCommand>
{
    public AddMeasurementCommandValidator()
    {
        RuleFor(x => x.MeasuredAt)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Thời gian đo không được ở tương lai.");

        RuleFor(x => x)
            .Must(x => x.WeightKg.HasValue || x.HeightCm.HasValue || x.HeartRateBpm.HasValue
                       || x.BodyTemperature.HasValue || x.BloodGlucose.HasValue || x.Spo2Percent.HasValue)
            .WithMessage("Phải nhập ít nhất một chỉ số đo lường.");

        When(x => x.WeightKg.HasValue, () =>
            RuleFor(x => x.WeightKg).InclusiveBetween(1m, 500m).WithMessage("Cân nặng không hợp lệ."));

        When(x => x.HeightCm.HasValue, () =>
            RuleFor(x => x.HeightCm).InclusiveBetween(30m, 250m).WithMessage("Chiều cao không hợp lệ."));

        When(x => x.Spo2Percent.HasValue, () =>
            RuleFor(x => x.Spo2Percent).InclusiveBetween(50m, 100m).WithMessage("SpO2 không hợp lệ."));
    }
}
