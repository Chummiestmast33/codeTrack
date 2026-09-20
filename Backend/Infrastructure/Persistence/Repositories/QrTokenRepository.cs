using Backend.Application.Abstractions;
using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

public sealed class QrTokenRepository : IQrTokenRepository
{
    private readonly TallerDbContext _db;

    public QrTokenRepository(TallerDbContext db)
    {
        _db = db;
    }

    public Task<QrToken?> GetByTokenAsync(string token, CancellationToken cancellationToken) =>
        _db.QrTokens.FirstOrDefaultAsync(q => q.Token == token, cancellationToken);

    public Task<QrToken?> GetLatestBySessionAsync(Guid sessionId, CancellationToken cancellationToken) =>
        _db.QrTokens
            .Where(q => q.SessionId == sessionId)
            .OrderByDescending(q => q.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(QrToken token, CancellationToken cancellationToken) =>
        await _db.QrTokens.AddAsync(token, cancellationToken);
}
