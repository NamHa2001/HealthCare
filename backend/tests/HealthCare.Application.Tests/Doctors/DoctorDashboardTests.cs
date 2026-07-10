using System.Text.Json;
using FluentAssertions;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Doctors.Dashboard.Commands;
using HealthCare.Application.Doctors.Dashboard.Common;
using HealthCare.Application.Doctors.Dashboard.Queries;
using HealthCare.Application.Tests.Helpers;
using HealthCare.Domain.Entities.Auth;
using HealthCare.Domain.Entities.Doctors;
using HealthCare.Domain.Entities.HealthProfile;
using HealthCare.Domain.Enums;
using Moq;

namespace HealthCare.Application.Tests.Doctors;

public class DoctorDashboardTests
{
    private readonly Mock<ICurrentUser> _currentUser = new();

    private sealed record Ctx(
        HealthCare.Infrastructure.Persistence.ApplicationDbContext Db,
        User Doctor, User Patient, Guid ProfileId, PatientDoctorLink Link);

    private async Task<Ctx> SeedAsync(params string[] scopes)
    {
        var db = TestDbContextFactory.Create();

        var doctor = User.Create("bs@test.com", "hash", "Lan", "Pham");
        var patient = User.Create("bn@test.com", "hash", "Hung", "Vo");
        db.Users.AddRange(doctor, patient);

        var docProfile = DoctorProfile.Create(doctor.Id, "CCHN-888", "Tim mạch", "BV E", "[]");
        docProfile.Approve(Guid.NewGuid());
        db.DoctorProfiles.Add(docProfile);

        var profile = HealthProfile.CreateForUser(patient.Id);
        db.HealthProfiles.Add(profile);

        var link = PatientDoctorLink.Create(doctor.Id, profile.Id, "doctor");
        link.Accept(JsonSerializer.Serialize(scopes), "consent text", null, null, patient.Id);
        db.PatientDoctorLinks.Add(link);

        await db.SaveChangesAsync();
        _currentUser.Setup(u => u.UserId).Returns(doctor.Id);
        return new Ctx(db, doctor, patient, profile.Id, link);
    }

    [Fact]
    public async Task EnsureLinked_ActiveLinkInScope_WritesAuditLog()
    {
        var ctx = await SeedAsync("measurements");

        await DoctorAccess.EnsureLinkedAsync(ctx.Db, ctx.Doctor.Id, ctx.ProfileId,
            "measurements", "doctor_patient_measurements", CancellationToken.None);

        var audit = ctx.Db.AuditLogs.Single();
        audit.EventType.Should().Be("data_access");
        audit.UserId.Should().Be(ctx.Doctor.Id);
        audit.TargetUserId.Should().Be(ctx.Patient.Id);
    }

    [Fact]
    public async Task EnsureLinked_ScopeNotGranted_ThrowsForbidden()
    {
        var ctx = await SeedAsync("vaccines"); // không có measurements

        var act = () => DoctorAccess.EnsureLinkedAsync(ctx.Db, ctx.Doctor.Id, ctx.ProfileId,
            "measurements", "x", CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task EnsureLinked_RevokedLink_ThrowsForbidden()
    {
        var ctx = await SeedAsync("measurements");
        ctx.Link.Revoke("patient");
        await ctx.Db.SaveChangesAsync();

        var act = () => DoctorAccess.EnsureLinkedAsync(ctx.Db, ctx.Doctor.Id, ctx.ProfileId,
            "measurements", "x", CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task EnsureLinked_UnlinkedProfile_ThrowsForbidden()
    {
        var ctx = await SeedAsync("measurements");
        var other = HealthProfile.CreateForUser(Guid.NewGuid());
        ctx.Db.HealthProfiles.Add(other);
        await ctx.Db.SaveChangesAsync();

        var act = () => DoctorAccess.EnsureLinkedAsync(ctx.Db, ctx.Doctor.Id, other.Id,
            null, "x", CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task GetMyPatients_RespectsConsentScope()
    {
        var ctx = await SeedAsync("vaccines"); // KHÔNG chia sẻ measurements/bp
        ctx.Db.HealthMeasurements.Add(HealthMeasurement.Create(
            ctx.ProfileId, DateTime.UtcNow, weightKg: 70, heightCm: 170));
        await ctx.Db.SaveChangesAsync();

        var result = await new GetMyPatientsQueryHandler(ctx.Db, _currentUser.Object)
            .Handle(new GetMyPatientsQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].LatestBmi.Should().BeNull();   // measurement tồn tại nhưng ngoài scope
        result[0].LatestBp.Should().BeNull();
    }

    [Fact]
    public async Task AcknowledgeAlert_MarksAcknowledged_AndAudits()
    {
        var ctx = await SeedAsync("measurements", "blood_pressure");
        var alert = HealthAlert.Create(ctx.ProfileId, AlertType.HighBP,
            AlertSeverity.Critical, "Huyết áp 190/120");
        ctx.Db.HealthAlerts.Add(alert);
        await ctx.Db.SaveChangesAsync();

        await new AcknowledgeAlertAsDoctorCommandHandler(ctx.Db, _currentUser.Object)
            .Handle(new AcknowledgeAlertAsDoctorCommand(ctx.ProfileId, alert.Id), CancellationToken.None);

        ctx.Db.HealthAlerts.Single().IsAcknowledged.Should().BeTrue();
        ctx.Db.AuditLogs.Should().Contain(a => a.Action == "acknowledge");
    }
}
