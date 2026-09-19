namespace Backend.Domain.Enums;

/// <summary>Student or administrator/instructor (RF-03).</summary>
public enum UserRole
{
    Student = 0,
    Administrator = 1
}

/// <summary>Registration workflow state (D-01).</summary>
public enum ApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

/// <summary>Session lifecycle (RF-06).</summary>
public enum SessionStatus
{
    Planned = 0,
    Imparted = 1,
    Cancelled = 2
}

/// <summary>Attendance marks, manual or via QR (RF-07).</summary>
public enum AttendanceStatus
{
    Present = 0,
    Absent = 1,
    Late = 2,
    Excused = 3
}

/// <summary>Activity lifecycle (RF-12).</summary>
public enum ActivityStatus
{
    Draft = 0,
    Published = 1,
    Closed = 2
}

/// <summary>Accepted delivery content per activity (RF-14, RN-07).</summary>
public enum SubmissionMode
{
    UrlOnly = 0,
    FileOnly = 1,
    UrlOrFile = 2,
    UrlAndFile = 3
}

/// <summary>Delivery state (RF-17).</summary>
public enum SubmissionStatus
{
    Pending = 0,
    Submitted = 1,
    Reviewed = 2,
    Incomplete = 3
}

/// <summary>Progress per topic (RF-19, D-05).</summary>
public enum ProgressStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2
}
