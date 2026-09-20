using Backend.Domain.Entities;

namespace Backend.Application.Abstractions;

public interface IActivityRepository
{
    Task<Activity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Activity>> ListAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Activity>> ListByTopicAsync(Guid topicId, CancellationToken cancellationToken);

    Task AddAsync(Activity activity, CancellationToken cancellationToken);

    void Update(Activity activity);
}
