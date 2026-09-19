using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Domain.Rules;

namespace Backend.Tests;

public sealed class AttendancePolicyTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 19, 2, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Qr_Requires_Approved_Active_Account()
    {
        var pending = User.RegisterStudent("q01", "Ana Paz", "a@x.com", "h", Now);
        Assert.False(AttendancePolicy.CanRegisterViaQr(pending));

        var approved = User.RegisterStudent("q02", "Luis Rey", "l@x.com", "h", Now);
        approved.Approve(Now);
        Assert.True(AttendancePolicy.CanRegisterViaQr(approved));

        approved.Deactivate(Now);
        Assert.False(AttendancePolicy.CanRegisterViaQr(approved));
    }

    [Fact]
    public void Duplicate_Attendance_Is_Rejected()
    {
        AttendancePolicy.EnsureNoDuplicate(alreadyExists: false);
        Assert.Throws<InvalidOperationException>(() => AttendancePolicy.EnsureNoDuplicate(alreadyExists: true));
    }

    [Fact]
    public void QrToken_Expiry_Is_Decided_In_Utc()
    {
        var createdAt = Now.AddHours(-1);
        var token = QrToken.Create(Guid.NewGuid(), QrToken.GenerateToken(),
            expiresAt: new DateTimeOffset(2026, 9, 19, 2, 0, 0, TimeSpan.Zero), createdAt);

        Assert.False(token.IsExpired(new DateTimeOffset(2026, 9, 18, 19, 59, 0, TimeSpan.FromHours(-6))));
        Assert.True(token.IsExpired(new DateTimeOffset(2026, 9, 18, 20, 0, 0, TimeSpan.FromHours(-6))));
        Assert.Throws<ArgumentException>(() =>
            QrToken.Create(Guid.NewGuid(), "t", expiresAt: Now, Now));
    }
}
