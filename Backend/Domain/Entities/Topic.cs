namespace Backend.Domain.Entities;

/// <summary>Official workshop topic (RF-05).</summary>
public sealed class Topic : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public int OrderNumber { get; private set; }

    public bool IsActive { get; private set; } = true;

    private Topic()
    {
    }

    private Topic(string name, string? description, int orderNumber, DateTimeOffset now)
        : base(now)
    {
        Name = name;
        Description = description;
        OrderNumber = orderNumber;
    }

    public static Topic Create(string? name, string? description, int orderNumber, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Topic(name.Trim(), description, orderNumber, now);
    }

    public void Rename(string? name, string? description, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Description = description;
        Touch(now);
    }

    public void Reorder(int orderNumber, DateTimeOffset now)
    {
        OrderNumber = orderNumber;
        Touch(now);
    }

    public void Deactivate(DateTimeOffset now)
    {
        IsActive = false;
        Touch(now);
    }

    public void Activate(DateTimeOffset now)
    {
        IsActive = true;
        Touch(now);
    }
}
