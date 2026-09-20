using Backend.Application.Abstractions;
using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

public sealed class AttendanceRepository : IAttendanceRepository
{
    private readonly TallerDbContext _db;

    public AttendanceRepository(TallerDbContext db)
    {
        _db = db;
    }

    public Task<AttendanceRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _db.AttendanceRecords.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<AttendanceRecord?> GetAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken) =>
        _db.AttendanceRecords.FirstOrDefaultAsync(a => a.SessionId == sessionId && a.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<AttendanceRecord>> ListBySessionAsync(Guid sessionId, CancellationToken cancellationToken) =>
        await _db.AttendanceRecords
            .Where(a => a.SessionId == sessionId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AttendanceRecord>> ListByUserAsync(Guid userId, CancellationToken cancellationToken) =>
        await _db.AttendanceRecords
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(AttendanceRecord record, CancellationToken cancellationToken) =>
        await _db.AttendanceRecords.AddAsync(record, cancellationToken);

    public void Update(AttendanceRecord record) => _db.AttendanceRecords.Update(record);
}
