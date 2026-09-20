using Backend.Domain.Entities;

namespace Backend.Application.Abstractions;

public interface IQrTokenRepository
{
    Task<QrToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);

    Task<QrToken?> GetLatestBySessionAsync(Guid sessionId, CancellationToken cancellationToken);

    Task AddAsync(QrToken token, CancellationToken cancellationToken);
}
