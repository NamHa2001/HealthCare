using FluentAssertions;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Doctors.Commands.RegisterDoctor;
using HealthCare.Application.Doctors.Commands.VerifyDoctor;
using HealthCare.Application.Doctors.DTOs;
using HealthCare.Application.Tests.Helpers;
using HealthCare.Domain.Entities.Auth;
using HealthCare.Domain.Entities.Doctors;
using HealthCare.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace HealthCare.Application.Tests.Doctors;

public class DoctorRegistrationTests
{
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IFileStorageService> _storage = new();
    private readonly Mock<IEmailService> _email = new();

    public DoctorRegistrationTests()
    {
        _storage
            .Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => $"2026/07/{Guid.NewGuid()}.jpg");
    }

    private static RegisterDoctorCommand ValidCommand(string license = "CCHN-001234") =>
        new(license, "Nội tổng quát", "BV Bạch Mai",
            [new UploadedFile(new MemoryStream([1, 2, 3]), "cchn.jpg", "image/jpeg", 3)]);

    private async Task<(HealthCare.Infrastructure.Persistence.ApplicationDbContext db, User user)> SeedAsync()
    {
        var db = TestDbContextFactory.Create();
        var user = User.Create("doctor@test.com", "hash", "Minh", "Nguyen");
        db.Users.Add(user);
        db.Roles.Add(Role.Create("doctor", "Bác sĩ đã xác minh"));
        await db.SaveChangesAsync();
        _currentUser.Setup(u => u.UserId).Returns(user.Id);
        return (db, user);
    }

    [Fact]
    public async Task Register_CreatesPendingProfile_AndUploadsFiles()
    {
        var (db, _) = await SeedAsync();
        var handler = new RegisterDoctorCommandHandler(db, _currentUser.Object, _storage.Object);

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.Status.Should().Be("pending");
        db.DoctorProfiles.Single().Status.Should().Be(DoctorProfileStatus.Pending);
        db.DoctorProfiles.Single().LicenseDocKeys.Should().Contain(".jpg");
        _storage.Verify(s => s.UploadAsync(It.IsAny<Stream>(), "cchn.jpg", "image/jpeg", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_LicenseTakenByAnotherUser_ThrowsConflict()
    {
        var (db, _) = await SeedAsync();
        var otherUser = User.Create("other@test.com", "hash", "Khac", "Bs");
        db.Users.Add(otherUser);
        db.DoctorProfiles.Add(DoctorProfile.Create(
            otherUser.Id, "CCHN-001234", "Nhi", "BV Nhi TW", "[]"));
        await db.SaveChangesAsync();

        var handler = new RegisterDoctorCommandHandler(db, _currentUser.Object, _storage.Object);
        var act = () => handler.Handle(ValidCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Register_WhilePending_ThrowsConflict()
    {
        var (db, _) = await SeedAsync();
        var handler = new RegisterDoctorCommandHandler(db, _currentUser.Object, _storage.Object);
        await handler.Handle(ValidCommand(), CancellationToken.None);

        var act = () => handler.Handle(ValidCommand("CCHN-999999"), CancellationToken.None);
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Approve_SetsApproved_AndAssignsDoctorRole()
    {
        var (db, user) = await SeedAsync();
        var register = new RegisterDoctorCommandHandler(db, _currentUser.Object, _storage.Object);
        var profile = await register.Handle(ValidCommand(), CancellationToken.None);

        var adminId = Guid.NewGuid();
        _currentUser.Setup(u => u.UserId).Returns(adminId);
        var approve = new ApproveDoctorCommandHandler(db, _currentUser.Object, _email.Object,
            Mock.Of<ILogger<ApproveDoctorCommandHandler>>());
        await approve.Handle(new ApproveDoctorCommand(profile.Id), CancellationToken.None);

        var saved = db.DoctorProfiles.Single();
        saved.Status.Should().Be(DoctorProfileStatus.Approved);
        saved.VerifiedBy.Should().Be(adminId);
        db.UserRoles.Should().Contain(ur => ur.UserId == user.Id);
        _email.Verify(e => e.SendDoctorApprovedAsync(user.Email, user.FirstName, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reject_ThenResubmit_ReturnsToPending()
    {
        var (db, _) = await SeedAsync();
        var register = new RegisterDoctorCommandHandler(db, _currentUser.Object, _storage.Object);
        var profile = await register.Handle(ValidCommand(), CancellationToken.None);
        var doctorUserId = _currentUser.Object.UserId!.Value;

        // Admin từ chối
        _currentUser.Setup(u => u.UserId).Returns(Guid.NewGuid());
        var reject = new RejectDoctorCommandHandler(db, _currentUser.Object, _email.Object,
            Mock.Of<ILogger<RejectDoctorCommandHandler>>());
        await reject.Handle(new RejectDoctorCommand(profile.Id, "Ảnh CCHN mờ"), CancellationToken.None);

        db.DoctorProfiles.Single().Status.Should().Be(DoctorProfileStatus.Rejected);
        db.DoctorProfiles.Single().RejectReason.Should().Be("Ảnh CCHN mờ");

        // Bác sĩ nộp lại
        _currentUser.Setup(u => u.UserId).Returns(doctorUserId);
        var result = await register.Handle(ValidCommand(), CancellationToken.None);

        result.Status.Should().Be("pending");
        db.DoctorProfiles.Single().RejectReason.Should().BeNull();
    }

    [Fact]
    public async Task Suspend_RemovesDoctorRole()
    {
        var (db, user) = await SeedAsync();
        var register = new RegisterDoctorCommandHandler(db, _currentUser.Object, _storage.Object);
        var profile = await register.Handle(ValidCommand(), CancellationToken.None);

        _currentUser.Setup(u => u.UserId).Returns(Guid.NewGuid()); // admin
        await new ApproveDoctorCommandHandler(db, _currentUser.Object, _email.Object,
                Mock.Of<ILogger<ApproveDoctorCommandHandler>>())
            .Handle(new ApproveDoctorCommand(profile.Id), CancellationToken.None);
        db.UserRoles.Should().Contain(ur => ur.UserId == user.Id);

        await new SuspendDoctorCommandHandler(db, _currentUser.Object, _email.Object,
                Mock.Of<ILogger<SuspendDoctorCommandHandler>>())
            .Handle(new SuspendDoctorCommand(profile.Id, "Khiếu nại từ bệnh nhân"), CancellationToken.None);

        db.DoctorProfiles.Single().Status.Should().Be(DoctorProfileStatus.Suspended);
        db.UserRoles.Should().NotContain(ur => ur.UserId == user.Id);
    }
}
