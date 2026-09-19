using Backend.Domain.Enums;
using Backend.Domain.Rules;

namespace Backend.Domain.Entities;

/// <summary>Student or administrator account (RF-01 to RF-04, D-01).</summary>
public sealed class User : AuditableEntity
{
    public string ControlNumber { get; private set; } = string.Empty;

    public string FullName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; } = UserRole.Student;

    public ApprovalStatus ApprovalStatus { get; private set; } = ApprovalStatus.Pending;

    public bool IsActive { get; private set; } = true;

    private User()
    {
    }

    private User(string controlNumber, string fullName, string email, string passwordHash, UserRole role, DateTimeOffset now)
        : base(now)
    {
        ControlNumber = controlNumber;
        FullName = fullName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
    }

    public static User RegisterStudent(string? controlNumber, string? fullName, string? email, string? passwordHash, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new User(
            ControlNumberRules.Normalize(controlNumber),
            fullName.Trim(),
            email.Trim(),
            passwordHash,
            UserRole.Student,
            now);
    }

    public static User RegisterAdministrator(string? controlNumber, string? fullName, string? email, string? passwordHash, DateTimeOffset now)
    {
        var admin = RegisterStudent(controlNumber, fullName, email, passwordHash, now);
        admin.Role = UserRole.Administrator;
        return admin;
    }

    /// <summary>Only approved and active accounts may sign in (D-01).</summary>
    public bool CanSignIn => ApprovalStatus == ApprovalStatus.Approved && IsActive;

    public void Approve(DateTimeOffset now)
    {
        ApprovalStatus = ApprovalStatus.Approved;
        Touch(now);
    }

    public void Reject(DateTimeOffset now)
    {
        ApprovalStatus = ApprovalStatus.Rejected;
        Touch(now);
    }

    public void Activate(DateTimeOffset now)
    {
        IsActive = true;
        Touch(now);
    }

    public void Deactivate(DateTimeOffset now)
    {
        IsActive = false;
        Touch(now);
    }

    public void Rename(string? fullName, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        FullName = fullName.Trim();
        Touch(now);
    }

    /// <summary>Password reset by an administrator (D-02). Expects an already-hashed value.</summary>
    public void SetPasswordHash(string? passwordHash, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;
        Touch(now);
    }
}
