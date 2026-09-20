using Backend.Application.Abstractions;
using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

public sealed class ProgressRepository : IProgressRepository
{
    private readonly TallerDbContext _db;

    public ProgressRepository(TallerDbContext db)
    {
        _db = db;
    }

    public Task<ProgressRecord?> GetAsync(Guid userId, Guid topicId, CancellationToken cancellationToken) =>
        _db.ProgressRecords.FirstOrDefaultAsync(p => p.UserId == userId && p.TopicId == topicId, cancellationToken);

    public async Task<IReadOnlyList<ProgressRecord>> ListByUserAsync(Guid userId, CancellationToken cancellationToken) =>
        await _db.ProgressRecords.Where(p => p.UserId == userId).ToListAsync(cancellationToken);

    public async Task AddAsync(ProgressRecord record, CancellationToken cancellationToken) =>
        await _db.ProgressRecords.AddAsync(record, cancellationToken);

    public void Update(ProgressRecord record) => _db.ProgressRecords.Update(record);
}
