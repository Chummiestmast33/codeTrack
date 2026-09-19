using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Tests;

public sealed class UserRegistrationTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 19, 2, 0, 0, TimeSpan.Zero);

    [Fact]
    public void RegisterStudent_Starts_Pending_And_Cannot_Sign_In()
    {
        var user = User.RegisterStudent("  s001 ", "Ana Paz", "ana@example.com", "hash", Now);

        Assert.Equal("s001", user.ControlNumber);
        Assert.Equal(ApprovalStatus.Pending, user.ApprovalStatus);
        Assert.True(user.IsActive);
        Assert.False(user.CanSignIn);
    }

    [Fact]
    public void Approved_Active_User_Can_Sign_In()
    {
        var user = User.RegisterStudent("s002", "Luis Rey", "luis@example.com", "hash", Now);
        user.Approve(Now);

        Assert.True(user.CanSignIn);
    }

    [Fact]
    public void Rejected_Or_Deactivated_User_Cannot_Sign_In()
    {
        var rejected = User.RegisterStudent("s003", "Mia Sol", "mia@example.com", "hash", Now);
        rejected.Reject(Now);
        Assert.False(rejected.CanSignIn);

        var deactivated = User.RegisterStudent("s004", "Leo Mar", "leo@example.com", "hash", Now);
        deactivated.Approve(Now);
        deactivated.Deactivate(Now);
        Assert.False(deactivated.CanSignIn);
    }

    [Fact]
    public void RegisterStudent_Rejects_Missing_Control_Number()
    {
        Assert.Throws<ArgumentException>(() =>
            User.RegisterStudent("   ", "Ana Paz", "ana@example.com", "hash", Now));
    }
}
