using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations;

public sealed class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
    }
}

public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Title).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(32);
    }
}

public sealed class SessionTopicConfiguration : IEntityTypeConfiguration<SessionTopic>
{
    public void Configure(EntityTypeBuilder<SessionTopic> builder)
    {
        builder.HasKey(st => new { st.SessionId, st.TopicId });
    }
}

public sealed class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(a => new { a.SessionId, a.UserId }).IsUnique();
    }
}

public sealed class QrTokenConfiguration : IEntityTypeConfiguration<QrToken>
{
    public void Configure(EntityTypeBuilder<QrToken> builder)
    {
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Token).IsRequired().HasMaxLength(256);
        builder.HasIndex(q => q.Token).IsUnique();
    }
}

public sealed class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Title).IsRequired().HasMaxLength(200);
        builder.Property(a => a.MarkdownContent).IsRequired();
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(a => a.SubmissionMode).HasConversion<string>().HasMaxLength(32);
    }
}

public sealed class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(s => new { s.ActivityId, s.UserId, s.VersionNumber }).IsUnique();
    }
}

public sealed class ProgressRecordConfiguration : IEntityTypeConfiguration<ProgressRecord>
{
    public void Configure(EntityTypeBuilder<ProgressRecord> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.AutoStatus).HasConversion<string>().HasMaxLength(32);
        builder.Property(p => p.ManualStatus).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(p => new { p.UserId, p.TopicId }).IsUnique();
    }
}
