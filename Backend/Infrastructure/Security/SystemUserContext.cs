using Backend.Application.Abstractions;

namespace Backend.Infrastructure.Security;

/// <summary>System actor for background and design-time work (RF-24: null actor).</summary>
public sealed class SystemUserContext : IUserContext
{
    public static readonly SystemUserContext Instance = new();

    public Guid? CurrentUserId => null;
}
