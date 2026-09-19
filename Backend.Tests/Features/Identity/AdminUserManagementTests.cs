using Backend.Application.Common;
using Backend.Application.Features.Identity;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Tests.Features.Identity;

public sealed class AdminUserManagementTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 19, 2, 0, 0, TimeSpan.Zero);

    private sealed record Env(
        FakeUserRepository Users,
        FakeUnitOfWork UnitOfWork,
        ApproveUserHandler Approve,
        RejectUserHandler Reject,
        ActivateUserHandler Activate,
        DeactivateUserHandler Deactivate,
        ResetPasswordHandler Reset,
        GetUsersHandler GetUsers,
        GetPendingRegistrationsHandler GetPending,
        GetUserByIdHandler GetById);

    private static Env CreateEnv()
    {
        var users = new FakeUserRepository();
        var uow = new FakeUnitOfWork();
        var time = new FixedTimeProvider(Now);
        var hasher = new FakePasswordHasher();
        return new Env(
            users, uow,
            new ApproveUserHandler(users, uow, time),
            new RejectUserHandler(users, uow, time),
            new ActivateUserHandler(users, uow, time),
            new DeactivateUserHandler(users, uow, time),
            new ResetPasswordHandler(users, uow, hasher, time),
            new GetUsersHandler(users),
            new GetPendingRegistrationsHandler(users),
            new GetUserByIdHandler(users));
    }

    private static User SeedPending(FakeUserRepository users, string control = "a100")
    {
        var user = User.RegisterStudent(control, "Ana Paz", "a@x.com", "HASH:secret123", Now);
        users.Seed(user);
        return user;
    }

    [Fact]
    public async Task Approve_Enables_Sign_In_And_Persists()
    {
        var env = CreateEnv();
        var user = SeedPending(env.Users);

        var dto = await env.Approve.Handle(new ApproveUserCommand(user.Id), CancellationToken.None);

        Assert.Equal(ApprovalStatus.Approved, dto.ApprovalStatus);
        Assert.True((await env.Users.GetByIdAsync(user.Id, CancellationToken.None))!.CanSignIn);
        Assert.Equal(1, env.UnitOfWork.Saves);
    }

    [Fact]
    public async Task Reject_Blocks_Sign_In()
    {
        var env = CreateEnv();
        var user = SeedPending(env.Users);

        var dto = await env.Reject.Handle(new RejectUserCommand(user.Id), CancellationToken.None);

        Assert.Equal(ApprovalStatus.Rejected, dto.ApprovalStatus);
        Assert.False((await env.Users.GetByIdAsync(user.Id, CancellationToken.None))!.CanSignIn);
    }

    [Fact]
    public async Task Deactivate_And_Reactivate_Toggle_Access()
    {
        var env = CreateEnv();
        var user = SeedPending(env.Users);
        await env.Approve.Handle(new ApproveUserCommand(user.Id), CancellationToken.None);

        await env.Deactivate.Handle(new DeactivateUserCommand(user.Id), CancellationToken.None);
        Assert.False((await env.Users.GetByIdAsync(user.Id, CancellationToken.None))!.CanSignIn);

        await env.Activate.Handle(new ActivateUserCommand(user.Id), CancellationToken.None);
        Assert.True((await env.Users.GetByIdAsync(user.Id, CancellationToken.None))!.CanSignIn);
    }

    [Fact]
    public async Task ResetPassword_Rehashes_And_Allows_Login_With_New_Password()
    {
        var env = CreateEnv();
        var user = SeedPending(env.Users);
        var hasher = new FakePasswordHasher();

        await env.Reset.Handle(new ResetPasswordCommand(user.Id, "newsecret1"), CancellationToken.None);

        var stored = await env.Users.GetByIdAsync(user.Id, CancellationToken.None);
        Assert.NotNull(stored);
        Assert.True(hasher.Verify(stored.PasswordHash, "newsecret1"));
        Assert.False(hasher.Verify(stored.PasswordHash, "secret123"));
    }

    [Fact]
    public async Task Approve_Unknown_User_Throws_NotFound()
    {
        var env = CreateEnv();
        await Assert.ThrowsAsync<NotFoundException>(() =>
            env.Approve.Handle(new ApproveUserCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Queries_List_All_Pending_And_By_Id()
    {
        var env = CreateEnv();
        var pending = SeedPending(env.Users, "a101");
        var other = SeedPending(env.Users, "a102");
        await env.Approve.Handle(new ApproveUserCommand(other.Id), CancellationToken.None);

        var all = await env.GetUsers.Handle(new GetUsersQuery(), CancellationToken.None);
        Assert.Equal(2, all.Count);

        var pendings = await env.GetPending.Handle(new GetPendingRegistrationsQuery(), CancellationToken.None);
        var single = Assert.Single(pendings);
        Assert.Equal(pending.Id, single.Id);

        var byId = await env.GetById.Handle(new GetUserByIdQuery(pending.Id), CancellationToken.None);
        Assert.Equal("a101", byId.ControlNumber);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            env.GetById.Handle(new GetUserByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }
}
