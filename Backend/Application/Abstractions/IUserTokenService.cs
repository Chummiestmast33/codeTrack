using Backend.Domain.Entities;

namespace Backend.Application.Abstractions;

/// <summary>Signs authentication tokens. Real JWT implementation arrives with Infrastructure.</summary>
public interface IUserTokenService
{
    AuthToken GenerateToken(User user);
}
