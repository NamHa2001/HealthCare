using FluentAssertions;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Sharing.Commands.CreateShareGrant;
using HealthCare.Application.Sharing.Commands.RevokeShareGrant;
using HealthCare.Application.Sharing.Common;
using HealthCare.Application.Sharing.Queries.GetSharedData;
using HealthCare.Application.Tests.Helpers;
using HealthCare.Domain.Entities.Auth;
using HealthCare.Domain.Entities.HealthProfile;
using Moq;

namespace HealthCare.Application.Tests.Sharing;

public class ShareGrantTests
{
    private readonly Mock<ICurrentUser> _currentUserMock = new();

    private async Task<(HealthCare.Infrastructure.Persistence.ApplicationDbContext db, Guid userId, Guid profileId)> SeedAsync()
    {
        var db = TestDbContextFactory.Create();
        var user = User.Create("share@test.com", "hash", "Share", "Tester");
        db.Users.Add(user);
        var profile = HealthProfile.CreateForUser(user.Id);
        db.HealthProfiles.Add(profile);
        await db.SaveChangesAsync();
        _currentUserMock.Setup(u => u.UserId).Returns(user.Id);
        return (db, user.Id, profile.Id);
    }

    [Fact]
    public async Task Create_ReturnsRawToken_AndStoresOnlyHash()
    {
        var (db, _, profileId) = await SeedAsync();
        var handler = new CreateShareGrantCommandHandler(db, _currentUserMock.Object);

        var result = await handler.Handle(
            new CreateShareGrantCommand(profileId, ["measurements"], 24), CancellationToken.None);

        result.Token.Should().HaveLength(64);
        var grant = db.ShareGrants.Single();
        grant.TokenHash.Should().NotBe(result.Token);
        grant.TokenHash.Should().Be(ShareTokens.Hash(result.Token));
        grant.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Create_FourthActiveGrant_ThrowsConflict()
    {
        var (db, _, profileId) = await SeedAsync();
        var handler = new CreateShareGrantCommandHandler(db, _currentUserMock.Object);
        var cmd = new CreateShareGrantCommand(profileId, ["measurements"], 24);

        for (var i = 0; i < 3; i++)
            await handler.Handle(cmd, CancellationToken.None);

        var act = () => handler.Handle(cmd, CancellationToken.None);
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Create_ProfileOfAnotherUser_ThrowsForbidden()
    {
        var (db, _, profileId) = await SeedAsync();
        _currentUserMock.Setup(u => u.UserId).Returns(Guid.NewGuid()); // user khác

        var handler = new CreateShareGrantCommandHandler(db, _currentUserMock.Object);
        var act = () => handler.Handle(
            new CreateShareGrantCommand(profileId, ["measurements"], 24), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task GetMeta_ValidToken_ReturnsOwnerAndIncrementsAccessCount()
    {
        var (db, _, profileId) = await SeedAsync();
        var create = new CreateShareGrantCommandHandler(db, _currentUserMock.Object);
        var created = await create.Handle(
            new CreateShareGrantCommand(profileId, ["measurements"], 24), CancellationToken.None);

        var meta = await new GetSharedMetaQueryHandler(db)
            .Handle(new GetSharedMetaQuery(created.Token, "1.2.3.4"), CancellationToken.None);

        meta.OwnerName.Should().Be("Share Tester");
        meta.Scope.Should().BeEquivalentTo(["measurements"]);
        db.ShareGrants.Single().AccessCount.Should().Be(1);
        db.ShareGrants.Single().LastAccessedIp.Should().Be("1.2.3.4");
    }

    [Fact]
    public async Task GetMeta_RevokedToken_ThrowsNotFound()
    {
        var (db, _, profileId) = await SeedAsync();
        var create = new CreateShareGrantCommandHandler(db, _currentUserMock.Object);
        var created = await create.Handle(
            new CreateShareGrantCommand(profileId, ["measurements"], 24), CancellationToken.None);

        await new RevokeShareGrantCommandHandler(db, _currentUserMock.Object)
            .Handle(new RevokeShareGrantCommand(created.Id), CancellationToken.None);

        var act = () => new GetSharedMetaQueryHandler(db)
            .Handle(new GetSharedMetaQuery(created.Token, null), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetVaccines_ScopeNotGranted_ThrowsNotFound()
    {
        var (db, _, profileId) = await SeedAsync();
        var create = new CreateShareGrantCommandHandler(db, _currentUserMock.Object);
        var created = await create.Handle(
            new CreateShareGrantCommand(profileId, ["measurements"], 24), CancellationToken.None);

        var act = () => new GetSharedVaccinesQueryHandler(db)
            .Handle(new GetSharedVaccinesQuery(created.Token), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetVaccines_ScopeGranted_ReturnsData()
    {
        var (db, _, profileId) = await SeedAsync();
        var create = new CreateShareGrantCommandHandler(db, _currentUserMock.Object);
        var created = await create.Handle(
            new CreateShareGrantCommand(profileId, ["vaccines"], 24), CancellationToken.None);

        var result = await new GetSharedVaccinesQueryHandler(db)
            .Handle(new GetSharedVaccinesQuery(created.Token), CancellationToken.None);

        result.Should().BeEmpty(); // chưa có record nào nhưng không throw
    }
}
