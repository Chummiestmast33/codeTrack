using Backend.Application.Features.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Tests.Api;

public sealed class AuthControllerTests
{
    private static UserDto SampleUser() => new(
        Guid.NewGuid(), "s200", "Ana Paz", "a@x.com",
        Domain.Enums.UserRole.Student, Domain.Enums.ApprovalStatus.Pending, true, DateTimeOffset.UtcNow);

    [Fact]
    public async Task Register_Maps_Request_To_Command_And_Returns_201()
    {
        var dto = SampleUser();
        var sender = new CapturingSender(_ => Task.FromResult<object?>(dto));
        var controller = new Controllers.AuthController(sender);

        var result = await controller.Register(
            new Controllers.RegisterRequest(" s200 ", "Ana Paz", "a@x.com", "secret123"),
            CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
        var command = Assert.IsType<RegisterStudentCommand>(sender.LastRequest);
        Assert.Equal(" s200 ", command.ControlNumber);
        Assert.Equal(dto, created.Value);
    }

    [Fact]
    public async Task Login_Maps_Request_To_Command_And_Returns_200()
    {
        var auth = new AuthResultDto("tok", DateTimeOffset.UtcNow.AddHours(1), SampleUser());
        var sender = new CapturingSender(_ => Task.FromResult<object?>(auth));
        var controller = new Controllers.AuthController(sender);

        var result = await controller.Login(
            new Controllers.LoginRequest("s200", "secret123"), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var command = Assert.IsType<LoginCommand>(sender.LastRequest);
        Assert.Equal("s200", command.ControlNumber);
        Assert.Equal(auth, ok.Value);
    }

    [Fact]
    public async Task Admin_Approve_Maps_Id_To_Command()
    {
        var dto = SampleUser();
        var sender = new CapturingSender(_ => Task.FromResult<object?>(dto));
        var controller = new Controllers.Admin.UsersAdminController(sender);
        var id = Guid.NewGuid();

        var result = await controller.Approve(id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(id, Assert.IsType<ApproveUserCommand>(sender.LastRequest).UserId);
        Assert.Equal(dto, ok.Value);
    }

    [Fact]
    public async Task Admin_ResetPassword_Maps_Id_And_Password()
    {
        var dto = SampleUser();
        var sender = new CapturingSender(_ => Task.FromResult<object?>(dto));
        var controller = new Controllers.Admin.UsersAdminController(sender);
        var id = Guid.NewGuid();

        var result = await controller.ResetPassword(
            id, new Controllers.ResetPasswordRequest("newsecret1"), CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
        var command = Assert.IsType<ResetPasswordCommand>(sender.LastRequest);
        Assert.Equal((id, "newsecret1"), (command.UserId, command.NewPassword));
    }
}
