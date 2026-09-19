using Backend.Domain.Entities;

namespace Backend.Domain.Rules;

/// <summary>
/// Attendance guards: QR requires an approved and active signed-in
/// student (D-04); a single record per student and session (RN-04).
/// </summary>
public static class AttendancePolicy
{
    public static bool CanRegisterViaQr(User user) => user.CanSignIn;

    public static void EnsureNoDuplicate(bool alreadyExists)
    {
        if (alreadyExists)
        {
            throw new InvalidOperationException("Attendance is already registered for this student and session.");
        }
    }
}
