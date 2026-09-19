using Backend.Domain.Enums;

namespace Backend.Domain.Entities;

/// <summary>One attendance row per student and session (RF-07, RN-04).</summary>
public sealed class AttendanceRecord : AuditableEntity
{
    public Guid SessionId { get; private set; }

    public Guid UserId { get; private set; }

    public AttendanceStatus Status { get; private set; }

    public string? Observation { get; private set; }

    public Guid RegisteredBy { get; private set; }

    private AttendanceRecord()
    {
    }

    private AttendanceRecord(Guid sessionId, Guid userId, AttendanceStatus status, Guid registeredBy, string? observation, DateTimeOffset now)
        : base(now)
    {
        SessionId = sessionId;
        UserId = userId;
        Status = status;
        RegisteredBy = registeredBy;
        Observation = observation;
    }

    public static AttendanceRecord Register(Guid sessionId, Guid userId, AttendanceStatus status, Guid registeredBy, DateTimeOffset now, string? observation = null)
    {
        if (sessionId == Guid.Empty)
        {
            throw new ArgumentException("Session is required.", nameof(sessionId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User is required.", nameof(userId));
        }

        if (registeredBy == Guid.Empty)
        {
            throw new ArgumentException("Registrar is required.", nameof(registeredBy));
        }

        return new AttendanceRecord(sessionId, userId, status, registeredBy, observation, now);
    }

    public void UpdateStatus(AttendanceStatus status, string? observation, DateTimeOffset now)
    {
        Status = status;
        Observation = observation;
        Touch(now);
    }
}
