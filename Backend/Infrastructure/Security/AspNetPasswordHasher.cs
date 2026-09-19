using Backend.Application.Abstractions;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Backend.Infrastructure.Security;

/// <summary>Framework-backed password hashing; no extra dependency.</summary>
public sealed class AspNetPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string password) =>
        _hasher.HashPassword(null!, password);

    public bool Verify(string passwordHash, string password) =>
        _hasher.VerifyHashedPassword(null!, passwordHash, password)
            is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
}
