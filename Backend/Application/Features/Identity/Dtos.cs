using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.Features.Identity;

/// <summary>Identity DTOs. Password hashes are never exposed.</summary>
public sealed record UserDto(
    Guid Id,
    string ControlNumber,
    string FullName,
    string Email,
    UserRole Role,
    ApprovalStatus ApprovalStatus,
    bool IsActive,
    DateTimeOffset CreatedAt)
{
    public static UserDto From(User user) => new(
        user.Id,
        user.ControlNumber,
        user.FullName,
        user.Email,
        user.Role,
        user.ApprovalStatus,
        user.IsActive,
        user.CreatedAt);
}

public sealed record AuthResultDto(string Token, DateTimeOffset ExpiresAt, UserDto User);
