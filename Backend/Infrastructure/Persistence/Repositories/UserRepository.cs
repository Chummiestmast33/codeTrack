using Backend.Application.Abstractions;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly TallerDbContext _db;

    public UserRepository(TallerDbContext db)
    {
        _db = db;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByControlNumberAsync(string controlNumber, CancellationToken cancellationToken) =>
        _db.Users.FirstOrDefaultAsync(u => u.ControlNumber == controlNumber, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        _db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken) =>
        await _db.Users.OrderBy(u => u.FullName).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<User>> ListPendingAsync(CancellationToken cancellationToken) =>
        await _db.Users
            .Where(u => u.ApprovalStatus == ApprovalStatus.Pending)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken) =>
        await _db.Users.AddAsync(user, cancellationToken);

    public void Update(User user) => _db.Users.Update(user);
}
