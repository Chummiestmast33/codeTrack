using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.Features.Activities;

public sealed record ActivityDto(
    Guid Id,
    string Title,
    string MarkdownContent,
    Guid TopicId,
    string TopicName,
    Guid? SessionId,
    DateTimeOffset? DueDate,
    ActivityStatus Status,
    SubmissionMode SubmissionMode);
