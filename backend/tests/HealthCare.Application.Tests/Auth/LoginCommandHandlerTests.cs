using FluentAssertions;
using HealthCare.Application.Auth.Commands.Login;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Tests.Helpers;
using HealthCare.Domain.Entities.Auth;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HealthCare.Application.Tests.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<ITokenService> _tokenMock = new();
    private readonly Mock<IEmailService> _emailMock = new();

    private LoginCommandHandler CreateHandler(HealthCare.Infrastructure.Persistence.ApplicationDbContext db)
        => new(db, _tokenMock.Object, _emailMock.Object, NullLogger<LoginCommandHandler>.Instance);

    private static User CreateActiveUser(string email, string password)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        return User.Create(email, hash, "Test", "User");
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAuthResponse()
    {
        using var db = TestDbContextFactory.Create();

        var user = CreateActiveUser("test@example.com", "Secret123!");
        db.Users.Add(user);
        var role = Role.Create("user", "User");
        db.Roles.Add(role);
        await db.SaveChangesAsync();
        var userRole = UserRole.Create(user.Id, role.Id);
        db.UserRoles.Add(userRole);
        await db.SaveChangesAsync();

        _tokenMock.Setup(t => t.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()))
                  .Returns("access-token");
        _tokenMock.Setup(t => t.GenerateRefreshToken()).Returns("refresh-token");

        var handler = CreateHandler(db);
        var cmd = new LoginCommand("test@example.com", "Secret123!", "127.0.0.1");

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data!.AccessToken.Should().Be("access-token");
        result.Data.RefreshToken.Should().Be("refresh-token");
        result.Data.User.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsForbiddenException()
    {
        using var db = TestDbContextFactory.Create();
        db.Users.Add(CreateActiveUser("test@example.com", "CorrectPass!"));
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var cmd = new LoginCommand("test@example.com", "WrongPass!", null);

        var act = () => handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsForbiddenException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = CreateHandler(db);
        var cmd = new LoginCommand("nobody@example.com", "Pass123!", null);

        var act = () => handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_InactiveUser_ThrowsForbiddenException()
    {
        using var db = TestDbContextFactory.Create();
        var user = CreateActiveUser("locked@example.com", "Secret123!");
        user.Deactivate();
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var cmd = new LoginCommand("locked@example.com", "Secret123!", null);

        var act = () => handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>()
                 .WithMessage("*vô hiệu hóa*");
    }

    [Fact]
    public async Task Handle_LockedOutUser_ThrowsForbiddenWithHoursRemaining()
    {
        using var db = TestDbContextFactory.Create();
        var user = CreateActiveUser("brute@example.com", "Secret123!");
        // Simulate 10 failed attempts → locks 24h
        for (var i = 0; i < 10; i++) user.RecordFailedLogin();
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var cmd = new LoginCommand("brute@example.com", "Secret123!", null);

        var act = () => handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>()
                 .WithMessage("*tạm khóa*");
    }

    [Fact]
    public async Task Handle_WrongPassword_IncrementsFailedLoginCount()
    {
        using var db = TestDbContextFactory.Create();
        var user = CreateActiveUser("count@example.com", "CorrectPass!");
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var cmd = new LoginCommand("count@example.com", "WrongPass!", null);

        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(cmd, CancellationToken.None));
        await db.Entry(user).ReloadAsync();
        user.FailedLoginCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_SuccessfulLogin_ResetsFailedLoginCount()
    {
        using var db = TestDbContextFactory.Create();
        var user = CreateActiveUser("reset@example.com", "Secret123!");
        user.RecordFailedLogin();
        user.RecordFailedLogin();
        var role = Role.Create("user", "User");
        db.Roles.Add(role);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        db.UserRoles.Add(UserRole.Create(user.Id, role.Id));
        await db.SaveChangesAsync();

        _tokenMock.Setup(t => t.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IEnumerable<string>>())).Returns("tok");
        _tokenMock.Setup(t => t.GenerateRefreshToken()).Returns("ref");

        var handler = CreateHandler(db);
        await handler.Handle(new LoginCommand("reset@example.com", "Secret123!", null), CancellationToken.None);

        await db.Entry(user).ReloadAsync();
        user.FailedLoginCount.Should().Be(0);
    }
}
