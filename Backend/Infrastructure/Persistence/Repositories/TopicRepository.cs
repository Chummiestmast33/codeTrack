using Backend.Application.Abstractions;
using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

public sealed class TopicRepository : ITopicRepository
{
    private readonly TallerDbContext _db;

    public TopicRepository(TallerDbContext db)
    {
        _db = db;
    }

    public Task<Topic?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _db.Topics.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Topic>> ListOrderedAsync(CancellationToken cancellationToken) =>
        await _db.Topics.OrderBy(t => t.OrderNumber).ToListAsync(cancellationToken);

    public async Task AddAsync(Topic topic, CancellationToken cancellationToken) =>
        await _db.Topics.AddAsync(topic, cancellationToken);

    public void Update(Topic topic) => _db.Topics.Update(topic);
}
