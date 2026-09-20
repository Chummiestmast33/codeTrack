using Backend.Application.Common;
using Backend.Application.Features.Users;
using Backend.Domain.Entities;
using Backend.Tests.Features.Identity;

namespace Backend.Tests.Features.Admin;

public sealed class RenameUserTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 19, 2, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Rename_Updates_FullName()
    {
        var users = new FakeUserRepository();
        var uow = new FakeUnitOfWork();
        var time = new FixedTimeProvider(Now);
        var handler = new RenameUserHandler(users, uow, time);
        var user = User.RegisterStudent("r100", "Old Name", "r@x.com", "h", Now);
        users.Seed(user);

        var dto = await handler.Handle(new RenameUserCommand(user.Id, "  New Name "), CancellationToken.None);

        Assert.Equal("New Name", dto.FullName);
        Assert.Equal("New Name", (await users.GetByIdAsync(user.Id, CancellationToken.None))!.FullName);
        Assert.Equal(1, uow.Saves);
    }

    [Fact]
    public async Task Rename_Unknown_User_Throws_NotFound()
    {
        var handler = new RenameUserHandler(new FakeUserRepository(), new FakeUnitOfWork(), new FixedTimeProvider(Now));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new RenameUserCommand(Guid.NewGuid(), "Name"), CancellationToken.None));
    }

    [Fact]
    public void Validator_Rejects_Empty_Name()
    {
        var validator = new RenameUserValidator();
        Assert.False(validator.Validate(new RenameUserCommand(Guid.NewGuid(), "  ")).IsValid);
    }
}
