using Backend.Domain.Entities;

namespace Backend.Application.Abstractions;

public interface ITopicRepository
{
    Task<Topic?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Topic>> ListOrderedAsync(CancellationToken cancellationToken);

    Task AddAsync(Topic topic, CancellationToken cancellationToken);

    void Update(Topic topic);
}
