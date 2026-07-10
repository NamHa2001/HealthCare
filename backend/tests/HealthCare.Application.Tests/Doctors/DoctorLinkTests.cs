using FluentAssertions;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Doctors.Commands.VerifyDoctor;
using HealthCare.Application.Doctors.Links.Commands;
using HealthCare.Application.Tests.Helpers;
using HealthCare.Domain.Entities.Auth;
using HealthCare.Domain.Entities.Doctors;
using HealthCare.Domain.Entities.HealthProfile;
using HealthCare.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace HealthCare.Application.Tests.Doctors;

public class DoctorLinkTests
{
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IEmailService> _email = new();

    private sealed record Ctx(
        HealthCare.Infrastructure.Persistence.ApplicationDbContext Db,
        User Doctor, User Patient, Guid PatientProfileId);

    private async Task<Ctx> SeedAsync()
    {
        var db = TestDbContextFactory.Create();

        var doctor = User.Create("bs@test.com", "hash", "Lan", "Pham");
        var patient = User.Create("bn@test.com", "hash", "Hung", "Vo");
        db.Users.AddRange(doctor, patient);
        db.Roles.Add(Role.Create("doctor", "Bác sĩ"));

        var docProfile = DoctorProfile.Create(doctor.Id, "CCHN-777", "Nhi khoa", "BV Nhi TW", "[]");
        docProfile.Approve(Guid.NewGuid());
        db.DoctorProfiles.Add(docProfile);

        var patProfile = HealthProfile.CreateForUser(patient.Id);
        db.HealthProfiles.Add(patProfile);

        await db.SaveChangesAsync();
        return new Ctx(db, doctor, patient, patProfile.Id);
    }

    private InvitePatientCommandHandler InviteHandler(Ctx ctx) =>
        new(ctx.Db, _currentUser.Object, _email.Object, Mock.Of<ILogger<InvitePatientCommandHandler>>());

    [Fact]
    public async Task Invite_CreatesPendingLink_AndSendsEmail()
    {
        var ctx = await SeedAsync();
        _currentUser.Setup(u => u.UserId).Returns(ctx.Doctor.Id);

        await InviteHandler(ctx).Handle(new InvitePatientCommand("bn@test.com"), CancellationToken.None);

        var link = ctx.Db.PatientDoctorLinks.Single();
        link.Status.Should().Be(DoctorLinkStatus.Pending);
        link.InitiatedBy.Should().Be("doctor");
        _email.Verify(e => e.SendDoctorLinkInviteAsync(
            "bn@test.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Invite_UnverifiedDoctor_ThrowsForbidden()
    {
        var ctx = await SeedAsync();
        var fake = User.Create("fake@test.com", "hash", "Gia", "Mao");
        ctx.Db.Users.Add(fake);
        await ctx.Db.SaveChangesAsync();
        _currentUser.Setup(u => u.UserId).Returns(fake.Id); // không có DoctorProfile

        var act = () => InviteHandler(ctx).Handle(new InvitePatientCommand("bn@test.com"), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Accept_StoresImmutableConsentRecord()
    {
        var ctx = await SeedAsync();
        _currentUser.Setup(u => u.UserId).Returns(ctx.Doctor.Id);
        await InviteHandler(ctx).Handle(new InvitePatientCommand("bn@test.com"), CancellationToken.None);
        var linkId = ctx.Db.PatientDoctorLinks.Single().Id;

        _currentUser.Setup(u => u.UserId).Returns(ctx.Patient.Id);
        await new AcceptDoctorLinkCommandHandler(ctx.Db, _currentUser.Object).Handle(
            new AcceptDoctorLinkCommand(linkId, ["measurements", "vaccines"], "10.0.0.1", "TestAgent"),
            CancellationToken.None);

        var link = ctx.Db.PatientDoctorLinks.Single();
        link.Status.Should().Be(DoctorLinkStatus.Active);
        link.ConsentAt.Should().NotBeNull();
        link.ConsentIp.Should().Be("10.0.0.1");
        link.ConsentByUserId.Should().Be(ctx.Patient.Id);
        link.ConsentText.Should().Contain("CCHN-777").And.Contain("measurements");
    }

    [Fact]
    public async Task Accept_ByNonOwner_ThrowsForbidden()
    {
        var ctx = await SeedAsync();
        _currentUser.Setup(u => u.UserId).Returns(ctx.Doctor.Id);
        await InviteHandler(ctx).Handle(new InvitePatientCommand("bn@test.com"), CancellationToken.None);
        var linkId = ctx.Db.PatientDoctorLinks.Single().Id;

        var stranger = User.Create("stranger@test.com", "hash", "La", "Nguoi");
        ctx.Db.Users.Add(stranger);
        await ctx.Db.SaveChangesAsync();
        _currentUser.Setup(u => u.UserId).Returns(stranger.Id);

        var act = () => new AcceptDoctorLinkCommandHandler(ctx.Db, _currentUser.Object).Handle(
            new AcceptDoctorLinkCommand(linkId, ["measurements"], null, null), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Revoke_ByDoctor_SetsRevokedByDoctor()
    {
        var ctx = await SeedAsync();
        _currentUser.Setup(u => u.UserId).Returns(ctx.Doctor.Id);
        await InviteHandler(ctx).Handle(new InvitePatientCommand("bn@test.com"), CancellationToken.None);
        var linkId = ctx.Db.PatientDoctorLinks.Single().Id;

        await new RevokeDoctorLinkCommandHandler(ctx.Db, _currentUser.Object).Handle(
            new RevokeDoctorLinkCommand(linkId), CancellationToken.None);

        var link = ctx.Db.PatientDoctorLinks.Single();
        link.Status.Should().Be(DoctorLinkStatus.Revoked);
        link.RevokedBy.Should().Be("doctor");
    }

    [Fact]
    public async Task PatientInvite_RecordsConsentAtInviteTime_DoctorAcceptActivates()
    {
        var ctx = await SeedAsync();

        // Bệnh nhân mời — consent ghi ngay
        _currentUser.Setup(u => u.UserId).Returns(ctx.Patient.Id);
        await new InviteDoctorCommandHandler(ctx.Db, _currentUser.Object, _email.Object,
                Mock.Of<ILogger<InviteDoctorCommandHandler>>())
            .Handle(new InviteDoctorCommand(ctx.Doctor.Id, ctx.PatientProfileId,
                ["measurements", "visits"], "10.0.0.9", "UA"), CancellationToken.None);

        var link = ctx.Db.PatientDoctorLinks.Single();
        link.Status.Should().Be(DoctorLinkStatus.Pending);
        link.InitiatedBy.Should().Be("patient");
        link.ConsentAt.Should().NotBeNull();
        link.ConsentByUserId.Should().Be(ctx.Patient.Id);
        link.ConsentText.Should().Contain("CCHN-777");

        // Bác sĩ chấp nhận — kích hoạt, consent giữ nguyên
        var consentAtBefore = link.ConsentAt;
        _currentUser.Setup(u => u.UserId).Returns(ctx.Doctor.Id);
        await new AcceptDoctorLinkCommandHandler(ctx.Db, _currentUser.Object).Handle(
            new AcceptDoctorLinkCommand(link.Id, [], null, null), CancellationToken.None);

        link = ctx.Db.PatientDoctorLinks.Single();
        link.Status.Should().Be(DoctorLinkStatus.Active);
        link.ConsentAt.Should().Be(consentAtBefore);
    }

    [Fact]
    public async Task PatientInvite_StrangerDoctorCannotAccept()
    {
        var ctx = await SeedAsync();
        _currentUser.Setup(u => u.UserId).Returns(ctx.Patient.Id);
        await new InviteDoctorCommandHandler(ctx.Db, _currentUser.Object, _email.Object,
                Mock.Of<ILogger<InviteDoctorCommandHandler>>())
            .Handle(new InviteDoctorCommand(ctx.Doctor.Id, ctx.PatientProfileId,
                ["measurements"], null, null), CancellationToken.None);
        var linkId = ctx.Db.PatientDoctorLinks.Single().Id;

        _currentUser.Setup(u => u.UserId).Returns(Guid.NewGuid()); // không phải bác sĩ được mời
        var act = () => new AcceptDoctorLinkCommandHandler(ctx.Db, _currentUser.Object).Handle(
            new AcceptDoctorLinkCommand(linkId, [], null, null), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task SuspendDoctor_RevokesAllActiveLinks()
    {
        var ctx = await SeedAsync();
        _currentUser.Setup(u => u.UserId).Returns(ctx.Doctor.Id);
        await InviteHandler(ctx).Handle(new InvitePatientCommand("bn@test.com"), CancellationToken.None);
        var linkId = ctx.Db.PatientDoctorLinks.Single().Id;

        _currentUser.Setup(u => u.UserId).Returns(ctx.Patient.Id);
        await new AcceptDoctorLinkCommandHandler(ctx.Db, _currentUser.Object).Handle(
            new AcceptDoctorLinkCommand(linkId, ["measurements"], null, null), CancellationToken.None);

        // Admin suspend
        _currentUser.Setup(u => u.UserId).Returns(Guid.NewGuid());
        var docProfileId = ctx.Db.DoctorProfiles.Single().Id;
        await new SuspendDoctorCommandHandler(ctx.Db, _currentUser.Object, _email.Object,
                Mock.Of<ILogger<SuspendDoctorCommandHandler>>())
            .Handle(new SuspendDoctorCommand(docProfileId, "Vi phạm"), CancellationToken.None);

        var link = ctx.Db.PatientDoctorLinks.Single();
        link.Status.Should().Be(DoctorLinkStatus.Revoked);
        link.RevokedBy.Should().Be("system");
    }
}
