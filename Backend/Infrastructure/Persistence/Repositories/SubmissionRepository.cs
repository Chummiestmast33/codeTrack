using Backend.Application.Abstractions;
using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

public sealed class SubmissionRepository : ISubmissionRepository
{
    private readonly TallerDbContext _db;

    public SubmissionRepository(TallerDbContext db)
    {
        _db = db;
    }

    public Task<Submission?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _db.Submissions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<Submission?> GetLatestAsync(Guid activityId, Guid userId, CancellationToken cancellationToken) =>
        _db.Submissions
            .Where(s => s.ActivityId == activityId && s.UserId == userId)
            .OrderByDescending(s => s.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<Submission>> ListHistoryAsync(Guid activityId, Guid userId, CancellationToken cancellationToken) =>
        await _db.Submissions
            .Where(s => s.ActivityId == activityId && s.UserId == userId)
            .OrderBy(s => s.VersionNumber)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Submission>> ListByActivityAsync(Guid activityId, CancellationToken cancellationToken) =>
        await _db.Submissions
            .Where(s => s.ActivityId == activityId)
            .OrderBy(s => s.SubmittedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Submission>> ListByUserAsync(Guid userId, CancellationToken cancellationToken) =>
        await _db.Submissions
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.SubmittedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Submission submission, CancellationToken cancellationToken) =>
        await _db.Submissions.AddAsync(submission, cancellationToken);

    public void Update(Submission submission) => _db.Submissions.Update(submission);
}
