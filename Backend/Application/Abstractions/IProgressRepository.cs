using Backend.Domain.Entities;

namespace Backend.Application.Abstractions;

public interface IProgressRepository
{
    Task<ProgressRecord?> GetAsync(Guid userId, Guid topicId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProgressRecord>> ListByUserAsync(Guid userId, CancellationToken cancellationToken);

    Task AddAsync(ProgressRecord record, CancellationToken cancellationToken);

    void Update(ProgressRecord record);
}
