using Backend.Application.Abstractions;
using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

public sealed class ActivityRepository : IActivityRepository
{
    private readonly TallerDbContext _db;

    public ActivityRepository(TallerDbContext db)
    {
        _db = db;
    }

    public Task<Activity?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _db.Activities.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Activity>> ListAsync(CancellationToken cancellationToken) =>
        await _db.Activities.OrderBy(a => a.CreatedAt).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Activity>> ListByTopicAsync(Guid topicId, CancellationToken cancellationToken) =>
        await _db.Activities.Where(a => a.TopicId == topicId).OrderBy(a => a.CreatedAt).ToListAsync(cancellationToken);

    public async Task AddAsync(Activity activity, CancellationToken cancellationToken) =>
        await _db.Activities.AddAsync(activity, cancellationToken);

    public void Update(Activity activity) => _db.Activities.Update(activity);
}
