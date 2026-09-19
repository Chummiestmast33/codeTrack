namespace Backend.Domain.Entities;

/// <summary>Base with identity and UTC audit timestamps (RF-24, RN-09).</summary>
public abstract class AuditableEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public DateTimeOffset CreatedAt { get; protected set; }

    public DateTimeOffset UpdatedAt { get; protected set; }

    protected AuditableEntity()
    {
    }

    protected AuditableEntity(DateTimeOffset now)
    {
        CreatedAt = now;
        UpdatedAt = now;
    }

    protected void Touch(DateTimeOffset now) => UpdatedAt = now;
}
