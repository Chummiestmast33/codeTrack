using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.Features.Submissions;

public sealed record SubmissionDto(
    Guid Id,
    Guid ActivityId,
    Guid UserId,
    string? ControlNumber,
    string? FullName,
    string? Url,
    string? FileName,
    string? ContentType,
    long? FileSizeBytes,
    string? StoragePath,
    string? Comment,
    string? InstructorComment,
    SubmissionStatus Status,
    int VersionNumber,
    DateTimeOffset SubmittedAt,
    bool IsLate,
    DateTimeOffset? ReviewedAt);
