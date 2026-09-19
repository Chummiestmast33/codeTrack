using Backend.Domain.Entities;

namespace Backend.Application.Abstractions;

/// <summary>Persistence contract for users. Uniqueness is pre-checked here
/// and enforced with a unique constraint in PostgreSQL.</summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<User?> GetByControlNumberAsync(string controlNumber, CancellationToken cancellationToken);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<User>> ListPendingAsync(CancellationToken cancellationToken);

    Task AddAsync(User user, CancellationToken cancellationToken);

    void Update(User user);
}
