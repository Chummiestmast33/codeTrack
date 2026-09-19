using Backend.Domain.Enums;

namespace Backend.Domain.Rules;

/// <summary>
/// Automatic progress per topic (D-05): needs both attendance
/// and at least one reviewed activity. Neither = NotStarted,
/// only one = InProgress, both = Completed.
/// </summary>
public static class ProgressCalculator
{
    public static ProgressStatus Calculate(bool hasAttendance, bool hasReviewedActivity)
    {
        if (hasAttendance && hasReviewedActivity)
        {
            return ProgressStatus.Completed;
        }

        if (hasAttendance || hasReviewedActivity)
        {
            return ProgressStatus.InProgress;
        }

        return ProgressStatus.NotStarted;
    }
}
