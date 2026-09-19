using Backend.Application.Abstractions;

namespace Backend.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly TallerDbContext _db;

    public UnitOfWork(TallerDbContext db)
    {
        _db = db;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        _db.SaveChangesAsync(cancellationToken);
}
