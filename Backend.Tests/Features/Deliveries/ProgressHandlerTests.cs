using Backend.Application.Common;
using Backend.Application.Features.Progress;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Tests.Features.Catalog;
using Backend.Tests.Features.Identity;

namespace Backend.Tests.Features.Deliveries;

public sealed class ProgressHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 19, 2, 0, 0, TimeSpan.Zero);

    private sealed record Env(
        FakeUserRepository Users,
        FakeTopicRepository Topics,
        FakeSessionRepository Sessions,
        FakeActivityRepository Activities,
        FakeAttendanceRepository Attendance,
        FakeSubmissionRepository Submissions,
        FakeProgressRepository Progress,
        ProgressEvaluator Evaluator,
        GetStudentProgressHandler Query,
        AdjustProgressHandler Adjust,
        ClearProgressAdjustmentHandler Clear);

    private static Env CreateEnv()
    {
        var users = new FakeUserRepository();
        var topics = new FakeTopicRepository();
        var sessions = new FakeSessionRepository();
        var activities = new FakeActivityRepository();
        var attendance = new FakeAttendanceRepository();
        var submissions = new FakeSubmissionRepository();
        var progress = new FakeProgressRepository();
        var time = new FixedTimeProvider(Now);
        var evaluator = new ProgressEvaluator(topics, sessions, activities, attendance, submissions, progress);
        return new Env(
            users, topics, sessions, activities, attendance, submissions, progress, evaluator,
            new GetStudentProgressHandler(users, evaluator),
            new AdjustProgressHandler(progress, evaluator, time),
            new ClearProgressAdjustmentHandler(progress, evaluator, time));
    }

    private static User Student(FakeUserRepository users, string control = "p100")
    {
        var user = User.RegisterStudent(control, "N", control + "@x.com", "h", Now);
        user.Approve(Now);
        users.Seed(user);
        return user;
    }

    [Fact]
    public async Task Evaluate_Matrix_And_Manual_Override()
    {
        var env = CreateEnv();
        var user = Student(env.Users);
        var topic = Topic.Create("T1", null, 1, Now);
        env.Topics.Seed(topic);

        var start = await env.Query.Handle(new GetStudentProgressQuery(user.Id), CancellationToken.None);
        Assert.Equal(ProgressStatus.NotStarted, Assert.Single(start).EffectiveStatus);

        var session = Session.Create("S", null, Now, Guid.NewGuid(), Now);
        env.Sessions.Seed(session, [topic.Id]);
        env.Attendance.SeedFor(session.Id, user.Id);
        var partial = await env.Query.Handle(new GetStudentProgressQuery(user.Id), CancellationToken.None);
        Assert.Equal(ProgressStatus.InProgress, Assert.Single(partial).EffectiveStatus);

        var activity = Activity.Create("A", "md", topic.Id, null, null, SubmissionMode.UrlOnly, Now);
        activity.Publish(Now);
        env.Activities.Seed(activity);
        var reviewed = Submission.CreateFirst(activity.Id, user.Id, SubmissionMode.UrlOnly, "https://x", null, Now);
        reviewed.Review(Guid.NewGuid(), SubmissionStatus.Reviewed, Now);
        await env.Submissions.AddAsync(reviewed, CancellationToken.None);

        var full = await env.Query.Handle(new GetStudentProgressQuery(user.Id), CancellationToken.None);
        var single = Assert.Single(full);
        Assert.True(single.HasAttendance);
        Assert.True(single.HasReviewedActivity);
        Assert.Equal(ProgressStatus.Completed, single.EffectiveStatus);

        var adjusted = await env.Adjust.Handle(
            new AdjustProgressCommand(user.Id, topic.Id, ProgressStatus.InProgress, "motivo"),
            CancellationToken.None);
        Assert.Equal(ProgressStatus.InProgress, adjusted.EffectiveStatus);
        Assert.Equal("motivo", adjusted.AdjustmentReason);

        var cleared = await env.Clear.Handle(
            new ClearProgressAdjustmentCommand(user.Id, topic.Id), CancellationToken.None);
        Assert.Equal(ProgressStatus.Completed, cleared.EffectiveStatus);
        Assert.Null(cleared.ManualStatus);
    }

    [Fact]
    public async Task Query_Unknown_User_Throws_NotFound()
    {
        var env = CreateEnv();
        await Assert.ThrowsAsync<NotFoundException>(() =>
            env.Query.Handle(new GetStudentProgressQuery(Guid.NewGuid()), CancellationToken.None));
    }
}
