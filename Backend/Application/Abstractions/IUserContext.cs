namespace Backend.Application.Abstractions;

/// <summary>Authenticated actor for audit fields (RF-24). Null outside requests.</summary>
public interface IUserContext
{
    Guid? CurrentUserId { get; }
}
