using Backend.Application.Common;
using Backend.Application.Features.Identity;
using Backend.Domain.Entities;

namespace Backend.Tests.Features.Identity;

public sealed class LoginHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 19, 2, 0, 0, TimeSpan.Zero);

    private static (LoginHandler Handler, FakeUserRepository Users) CreateHandler()
    {
        var users = new FakeUserRepository();
        return (new LoginHandler(users, new FakePasswordHasher(), new FakeTokenService()), users);
    }

    private static User ApprovedUser(FakeUserRepository users, string control = "l100", string password = "secret123")
    {
        var user = User.RegisterStudent(control, "Luis Rey", "l@x.com", "HASH:" + password, Now);
        user.Approve(Now);
        users.Seed(user);
        return user;
    }

    [Fact]
    public async Task Login_Approved_User_Returns_Token()
    {
        var (handler, users) = CreateHandler();
        var user = ApprovedUser(users);

        var result = await handler.Handle(new LoginCommand("  l100 ", "secret123"), CancellationToken.None);

        Assert.Equal("TOKEN-" + user.Id.ToString("N"), result.Token);
        Assert.Equal(user.Id, result.User.Id);
    }

    [Fact]
    public async Task Login_Unknown_Control_Number_Throws_Unauthorized()
    {
        var (handler, _) = CreateHandler();
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            handler.Handle(new LoginCommand("missing", "secret123"), CancellationToken.None));
    }

    [Fact]
    public async Task Login_Wrong_Password_Throws_Unauthorized()
    {
        var (handler, users) = CreateHandler();
        ApprovedUser(users);
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            handler.Handle(new LoginCommand("l100", "wrongpass"), CancellationToken.None));
    }

    [Fact]
    public async Task Login_Pending_Account_Throws_Forbidden()
    {
        var (handler, users) = CreateHandler();
        users.Seed(User.RegisterStudent("l101", "Ana Paz", "a@x.com", "HASH:secret123", Now));

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new LoginCommand("l101", "secret123"), CancellationToken.None));
    }

    [Fact]
    public async Task Login_Deactivated_Account_Throws_Forbidden()
    {
        var (handler, users) = CreateHandler();
        var user = ApprovedUser(users);
        user.Deactivate(Now);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new LoginCommand("l100", "secret123"), CancellationToken.None));
    }
}
