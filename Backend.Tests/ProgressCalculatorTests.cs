using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Domain.Rules;

namespace Backend.Tests;

public sealed class ProgressCalculatorTests
{
    [Theory]
    [InlineData(false, false, ProgressStatus.NotStarted)]
    [InlineData(true, false, ProgressStatus.InProgress)]
    [InlineData(false, true, ProgressStatus.InProgress)]
    [InlineData(true, true, ProgressStatus.Completed)]
    public void Calculate_Follows_D05_Matrix(bool attendance, bool reviewed, ProgressStatus expected)
    {
        Assert.Equal(expected, ProgressCalculator.Calculate(attendance, reviewed));
    }

    [Fact]
    public void Manual_Adjustment_Wins_Until_Removed()
    {
        var record = ProgressRecord.Create(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
        record.ApplyAutomatic(hasAttendance: true, hasReviewedActivity: true, DateTimeOffset.UtcNow);
        Assert.Equal(ProgressStatus.Completed, record.EffectiveStatus);

        record.AdjustManually(ProgressStatus.InProgress, "Recupera evidencia", DateTimeOffset.UtcNow);
        Assert.Equal(ProgressStatus.InProgress, record.EffectiveStatus);

        record.ClearAdjustment(DateTimeOffset.UtcNow);
        Assert.Equal(ProgressStatus.Completed, record.EffectiveStatus);
    }

    [Fact]
    public void AdjustManually_Requires_Reason()
    {
        var record = ProgressRecord.Create(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
        Assert.Throws<ArgumentException>(() =>
            record.AdjustManually(ProgressStatus.Completed, "  ", DateTimeOffset.UtcNow));
    }
}
