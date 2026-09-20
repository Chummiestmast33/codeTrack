using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.Features.Sessions;

public sealed record TopicRefDto(Guid Id, string Name);

public sealed record SessionDto(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset SessionDate,
    SessionStatus Status,
    IReadOnlyList<TopicRefDto> Topics,
    DateTimeOffset CreatedAt);
