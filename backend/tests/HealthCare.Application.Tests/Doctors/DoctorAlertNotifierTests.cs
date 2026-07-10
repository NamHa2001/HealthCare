using System.Text.Json;
using FluentAssertions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Doctors.Notifications;
using HealthCare.Application.Tests.Helpers;
using HealthCare.Domain.Entities.Auth;
using HealthCare.Domain.Entities.Doctors;
using HealthCare.Domain.Entities.HealthProfile;
using HealthCare.Domain.Enums;
using HealthCare.Infrastructure.BackgroundJobs;
using Microsoft.Extensions.Logging;
using Moq;

namespace HealthCare.Application.Tests.Doctors;

public class DoctorAlertNotifierTests
{
    private readonly Mock<IEmailService> _email = new();
    private readonly Mock<INotificationService> _push = new();

    private sealed record Ctx(
        HealthCare.Infrastructure.Persistence.ApplicationDbContext Db,
        User Doctor, User Patient, Guid ProfileId);

    private async Task<Ctx> SeedAsync(params string[] scopes)
    {
        var db = TestDbContextFactory.Create();
        var doctor = User.Create("bs@test.com", "hash", "Lan", "Pham");
        var patient = User.Create("bn@test.com", "hash", "Hung", "Vo");
        db.Users.AddRange(doctor, patient);

        var profile = HealthProfile.CreateForUser(patient.Id);
        db.HealthProfiles.Add(profile);

        var link = PatientDoctorLink.Create(doctor.Id, profile.Id, "doctor");
        link.Accept(JsonSerializer.Serialize(scopes), "consent", null, null, patient.Id);
        db.PatientDoctorLinks.Add(link);

        await db.SaveChangesAsync();
        return new Ctx(db, doctor, patient, profile.Id);
    }

    private DoctorAlertNotifier Notifier(Ctx ctx) =>
        new(ctx.Db, _email.Object, _push.Object, Mock.Of<ILogger<DoctorAlertNotifier>>());

    private async Task<HealthAlert> AddAlertAsync(Ctx ctx, AlertSeverity severity)
    {
        var alert = HealthAlert.Create(ctx.ProfileId, AlertType.HighBP, severity, "Huyết áp cao: 190/120 mmHg");
        ctx.Db.HealthAlerts.Add(alert);
        await ctx.Db.SaveChangesAsync();
        return alert;
    }

    [Fact]
    public async Task Critical_SendsEmailImmediately_AndRecordsDelivery()
    {
        var ctx = await SeedAsync("blood_pressure");
        var alert = await AddAlertAsync(ctx, AlertSeverity.Critical);

        await Notifier(ctx).NotifyDoctorsAsync([alert], CancellationToken.None);

        _email.Verify(e => e.SendDoctorAlertAsync(
            ctx.Doctor.Email, ctx.Doctor.FirstName, "Hung Vo", alert.Message, It.IsAny<CancellationToken>()), Times.Once);
        var delivery = ctx.Db.DoctorAlertDeliveries.Single();
        delivery.Channel.Should().Be("email");
        delivery.Status.Should().Be("sent");
    }

    [Fact]
    public async Task Warning_QueuesForDigest_NoImmediateEmail()
    {
        var ctx = await SeedAsync("measurements");
        var alert = await AddAlertAsync(ctx, AlertSeverity.Warning);

        await Notifier(ctx).NotifyDoctorsAsync([alert], CancellationToken.None);

        _email.Verify(e => e.SendDoctorAlertAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        var delivery = ctx.Db.DoctorAlertDeliveries.Single();
        delivery.Channel.Should().Be("digest");
        delivery.Status.Should().Be("pending");
    }

    [Fact]
    public async Task DoctorWithoutHealthScope_NotNotified()
    {
        var ctx = await SeedAsync("vaccines"); // không có measurements/blood_pressure
        var alert = await AddAlertAsync(ctx, AlertSeverity.Critical);

        await Notifier(ctx).NotifyDoctorsAsync([alert], CancellationToken.None);

        ctx.Db.DoctorAlertDeliveries.Should().BeEmpty();
        _email.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task NotifyTwice_DoesNotDuplicateDelivery()
    {
        var ctx = await SeedAsync("blood_pressure");
        var alert = await AddAlertAsync(ctx, AlertSeverity.Critical);

        await Notifier(ctx).NotifyDoctorsAsync([alert], CancellationToken.None);
        await Notifier(ctx).NotifyDoctorsAsync([alert], CancellationToken.None);

        ctx.Db.DoctorAlertDeliveries.Should().HaveCount(1);
        _email.Verify(e => e.SendDoctorAlertAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DigestJob_GroupsPendingIntoOneEmail_AndMarksSent()
    {
        var ctx = await SeedAsync("measurements");
        var a1 = await AddAlertAsync(ctx, AlertSeverity.Warning);
        var a2 = await AddAlertAsync(ctx, AlertSeverity.Warning);
        await Notifier(ctx).NotifyDoctorsAsync([a1, a2], CancellationToken.None);
        ctx.Db.DoctorAlertDeliveries.Count(d => d.Status == "pending").Should().Be(2);

        var job = new DoctorDigestJob(ctx.Db, _email.Object, Mock.Of<ILogger<DoctorDigestJob>>());
        await job.SendDailyDigestsAsync();

        _email.Verify(e => e.SendDoctorDigestAsync(
            ctx.Doctor.Email, ctx.Doctor.FirstName,
            It.Is<IReadOnlyList<string>>(l => l.Count == 2), It.IsAny<CancellationToken>()), Times.Once);
        ctx.Db.DoctorAlertDeliveries.Where(d => d.Status == "sent").Should().HaveCount(2);
    }
}
