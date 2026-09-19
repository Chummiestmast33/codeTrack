using Backend.Application.Abstractions;
using Backend.Domain.Entities;

namespace Backend.Tests.Features.Identity;

public sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

public sealed class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password) => "HASH:" + password;

    public bool Verify(string passwordHash, string password) => passwordHash == "HASH:" + password;
}

public sealed class FakeTokenService : IUserTokenService
{
    public AuthToken GenerateToken(User user) =>
        new("TOKEN-" + user.Id.ToString("N"), new DateTimeOffset(2026, 9, 20, 2, 0, 0, TimeSpan.Zero));
}

public sealed class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<Guid, User> _users = [];

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_users.GetValueOrDefault(id));

    public Task<User?> GetByControlNumberAsync(string controlNumber, CancellationToken cancellationToken) =>
        Task.FromResult(_users.Values.FirstOrDefault(u => u.ControlNumber == controlNumber));

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        Task.FromResult(_users.Values.FirstOrDefault(u => u.Email == email));

    public Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<User>>(_users.Values.ToList());

    public Task<IReadOnlyList<User>> ListPendingAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<User>>(
            _users.Values.Where(u => u.ApprovalStatus == Domain.Enums.ApprovalStatus.Pending).ToList());

    public Task AddAsync(User user, CancellationToken cancellationToken)
    {
        _users[user.Id] = user;
        return Task.CompletedTask;
    }

    public void Update(User user) => _users[user.Id] = user;

    public void Seed(User user) => _users[user.Id] = user;
}

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public int Saves { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saves++;
        return Task.FromResult(1);
    }
}
