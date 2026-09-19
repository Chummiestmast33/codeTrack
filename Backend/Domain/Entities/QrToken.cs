namespace Backend.Domain.Entities;

/// <summary>Single-use expirable token binding a QR to a session (RF-08, RF-10).</summary>
public sealed class QrToken : AuditableEntity
{
    public Guid SessionId { get; private set; }

    public string Token { get; private set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; private set; }

    private QrToken()
    {
    }

    private QrToken(Guid sessionId, string token, DateTimeOffset expiresAt, DateTimeOffset now)
        : base(now)
    {
        SessionId = sessionId;
        Token = token;
        ExpiresAt = expiresAt;
    }

    public static QrToken Create(Guid sessionId, string? token, DateTimeOffset expiresAt, DateTimeOffset now)
    {
        if (sessionId == Guid.Empty)
        {
            throw new ArgumentException("Session is required.", nameof(sessionId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        if (expiresAt <= now)
        {
            throw new ArgumentException("Expiration must be in the future.", nameof(expiresAt));
        }

        return new QrToken(sessionId, token.Trim(), expiresAt, now);
    }

    public static string GenerateToken() => Guid.NewGuid().ToString("N");

    /// <summary>UTC comparison; the backend owns expiry decisions (RN-09).</summary>
    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;

    public bool IsValidForSession(Guid sessionId, DateTimeOffset now) =>
        SessionId == sessionId && !IsExpired(now);
}
