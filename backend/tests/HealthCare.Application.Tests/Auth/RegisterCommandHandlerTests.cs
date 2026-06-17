using FluentAssertions;
using HealthCare.Application.Auth.Commands.Register;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Tests.Helpers;
using HealthCare.Domain.Entities.Auth;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HealthCare.Application.Tests.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IEmailService> _emailMock = new();

    private RegisterCommandHandler CreateHandler(HealthCare.Infrastructure.Persistence.ApplicationDbContext db)
        => new(db, _emailMock.Object, NullLogger<RegisterCommandHandler>.Instance);

    [Fact]
    public async Task Handle_NewEmail_CreatesUserAndReturnsGuid()
    {
        using var db = TestDbContextFactory.Create();
        var handler = CreateHandler(db);
        var cmd = new RegisterCommand("new@example.com", "Pass@123!", "Nguyen", "Van A", null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBe(Guid.Empty);

        var user = db.Users.FirstOrDefault(u => u.Email == "new@example.com");
        user.Should().NotBeNull();
        user!.FirstName.Should().Be("Nguyen");
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsConflictException()
    {
        using var db = TestDbContextFactory.Create();
        db.Users.Add(User.Create("existing@example.com", "hash", "A", "B"));
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var cmd = new RegisterCommand("existing@example.com", "Pass@123!", "C", "D", null);

        var act = () => handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_EmailSendFails_StillReturnsSuccess()
    {
        using var db = TestDbContextFactory.Create();
        _emailMock.Setup(e => e.SendEmailVerificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ThrowsAsync(new Exception("SMTP down"));

        var handler = CreateHandler(db);
        var cmd = new RegisterCommand("smtp@example.com", "Pass@123!", "T", "T", null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeTrue("email failure should not block registration");
    }
}
