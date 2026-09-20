using Backend.Domain.Entities;

namespace Backend.Application.Abstractions;

public interface IAttendanceRepository
{
    Task<AttendanceRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<AttendanceRecord?> GetAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AttendanceRecord>> ListBySessionAsync(Guid sessionId, CancellationToken cancellationToken);

    Task AddAsync(AttendanceRecord record, CancellationToken cancellationToken);

    void Update(AttendanceRecord record);
}
