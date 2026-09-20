using Backend.Domain.Entities;

namespace Backend.Application.Abstractions;

public interface ISubmissionRepository
{
    Task<Submission?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Submission?> GetLatestAsync(Guid activityId, Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Submission>> ListHistoryAsync(Guid activityId, Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Submission>> ListByActivityAsync(Guid activityId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Submission>> ListByUserAsync(Guid userId, CancellationToken cancellationToken);

    Task AddAsync(Submission submission, CancellationToken cancellationToken);

    void Update(Submission submission);
}
