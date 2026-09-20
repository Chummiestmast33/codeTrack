using Backend.Application.Common;
using Backend.Application.Features.Submissions;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Tests.Features.Catalog;
using Backend.Tests.Features.Identity;
using Backend.Tests.Features.Submissions;

namespace Backend.Tests.Features.Deliveries;

public sealed class SubmissionsHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 19, 2, 0, 0, TimeSpan.Zero);

    private sealed record Env(
        FakeActivityRepository Activities,
        FakeSubmissionRepository Submissions,
        RequestUploadTicketHandler Ticket,
        SubmitActivityHandler Submit,
        GetMySubmissionHistoryHandler History,
        GetSubmissionsByActivityHandler ByActivity,
        ReviewSubmissionHandler Review,
        Guid TopicId);

    private static Env CreateEnv(SubmissionMode mode = SubmissionMode.UrlOrFile, DateTimeOffset? due = null)
    {
        var topics = new FakeTopicRepository();
        var topic = Topic.Create("T1", null, 1, Now);
        topics.Seed(topic);

        var activities = new FakeActivityRepository();
        var activity = Activity.Create("A1", "# md", topic.Id, null, due ?? Now.AddDays(7), mode, Now);
        activity.Publish(Now);
        activities.Seed(activity);

        var submissions = new FakeSubmissionRepository();
        var uow = new FakeUnitOfWork();
        var time = new FixedTimeProvider(Now);
        var enricher = new SubmissionEnricher();
        var storage = new FakeFileStorage(new FixedTimeProvider(Now));
        return new Env(
            activities, submissions,
            new RequestUploadTicketHandler(activities, submissions, storage),
            new SubmitActivityHandler(activities, submissions, uow, enricher, time),
            new GetMySubmissionHistoryHandler(activities, submissions, enricher),
            new GetSubmissionsByActivityHandler(activities, submissions, new FakeUserRepository(), enricher),
            new ReviewSubmissionHandler(activities, submissions, uow, enricher, time),
            topic.Id);
    }

    private static Guid ActivityId(Env env) =>
        env.Activities.ListAsync(CancellationToken.None).GetAwaiter().GetResult()[0].Id;

    private static readonly Guid Student = Guid.NewGuid();
    private static readonly Guid Reviewer = Guid.NewGuid();

    [Fact]
    public async Task Upload_Ticket_Computes_Next_Version_Key()
    {
        var env = CreateEnv();
        var ticket = await env.Ticket.Handle(
            new RequestUploadTicketCommand(ActivityId(env), Student, "main.py", "text/x-python", 1024),
            CancellationToken.None);

        Assert.Equal(1, ticket.VersionNumber);
        Assert.StartsWith("submissions/", ticket.StoragePath);
        Assert.EndsWith("/main.py", ticket.StoragePath);
    }

    [Fact]
    public async Task Submit_Creates_Versions_And_Flags_Late()
    {
        var env = CreateEnv(due: Now.AddHours(-1));

        var first = await env.Submit.Handle(
            new SubmitActivityCommand(ActivityId(env), Student, "https://github.com/u/r", null, null, null, null, null),
            CancellationToken.None);
        Assert.Equal(1, first.VersionNumber);
        Assert.True(first.IsLate);

        var second = await env.Submit.Handle(
            new SubmitActivityCommand(ActivityId(env), Student, "https://github.com/u/r2", null, null, null, null, null),
            CancellationToken.None);
        Assert.Equal(2, second.VersionNumber);

        var history = await env.History.Handle(
            new GetMySubmissionHistoryQuery(ActivityId(env), Student), CancellationToken.None);
        Assert.Equal([1, 2], history.Select(h => h.VersionNumber));
    }

    [Fact]
    public async Task Submit_Closed_Activity_Throws_Conflict()
    {
        var env = CreateEnv();
        var draft = Activity.Create("D", "md", env.TopicId, null, null, SubmissionMode.UrlOnly, Now);
        env.Activities.Seed(draft);

        await Assert.ThrowsAsync<ConflictException>(() => env.Submit.Handle(
            new SubmitActivityCommand(draft.Id, Student, "https://x", null, null, null, null, null),
            CancellationToken.None));
    }

    [Fact]
    public async Task Review_Sets_Status_And_Instructor_Comment()
    {
        var env = CreateEnv();
        var submitted = await env.Submit.Handle(
            new SubmitActivityCommand(ActivityId(env), Student, "https://github.com/u/r", null, null, null, null, null),
            CancellationToken.None);

        var reviewed = await env.Review.Handle(
            new ReviewSubmissionCommand(submitted.Id, SubmissionStatus.Reviewed, "Bien", Reviewer),
            CancellationToken.None);

        Assert.Equal(SubmissionStatus.Reviewed, reviewed.Status);
        Assert.Equal("Bien", reviewed.InstructorComment);
        Assert.NotNull(reviewed.ReviewedAt);

        var byActivity = await env.ByActivity.Handle(
            new GetSubmissionsByActivityQuery(ActivityId(env)), CancellationToken.None);
        Assert.Single(byActivity);
    }
}
