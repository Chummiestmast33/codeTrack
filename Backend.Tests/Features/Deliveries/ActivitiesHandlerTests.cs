using Backend.Application.Common;
using Backend.Application.Features.Activities;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Tests.Features.Catalog;
using Backend.Tests.Features.Identity;

namespace Backend.Tests.Features.Deliveries;

public sealed class ActivitiesHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 19, 2, 0, 0, TimeSpan.Zero);

    private sealed record Env(
        FakeTopicRepository Topics,
        FakeActivityRepository Activities,
        CreateActivityHandler Create,
        UpdateActivityHandler Update,
        PublishActivityHandler Publish,
        CloseActivityHandler Close,
        GetActivitiesHandler List,
        GetActivityByIdHandler ById);

    private static Env CreateEnv()
    {
        var activities = new FakeActivityRepository();
        var topics = new FakeTopicRepository();
        var uow = new FakeUnitOfWork();
        var time = new FixedTimeProvider(Now);
        var enricher = new ActivityEnricher();
        var topic = Topic.Create("T1", null, 1, Now);
        topics.Seed(topic);
        return new Env(
            topics,
            activities,
            new CreateActivityHandler(activities, topics, uow, enricher, time),
            new UpdateActivityHandler(activities, topics, uow, enricher, time),
            new PublishActivityHandler(activities, topics, uow, enricher, time),
            new CloseActivityHandler(activities, topics, uow, enricher, time),
            new GetActivitiesHandler(activities, topics, enricher),
            new GetActivityByIdHandler(activities, topics, enricher));
    }

    private static Guid TopicId(Env env) =>
        env.Topics.ListOrderedAsync(CancellationToken.None).GetAwaiter().GetResult()[0].Id;

    [Fact]
    public async Task Lifecycle_Draft_Published_Closed()
    {
        var env = CreateEnv();
        var topicId = TopicId(env);

        var created = await env.Create.Handle(
            new CreateActivityCommand("A1", "# Hola", topicId, null, Now.AddDays(7), SubmissionMode.UrlOrFile),
            CancellationToken.None);

        Assert.Equal(ActivityStatus.Draft, created.Status);
        Assert.Equal("T1", created.TopicName);
        Assert.False(created.DueDate is null);

        var published = await env.Publish.Handle(new PublishActivityCommand(created.Id), CancellationToken.None);
        Assert.Equal(ActivityStatus.Published, published.Status);

        var closed = await env.Close.Handle(new CloseActivityCommand(created.Id), CancellationToken.None);
        Assert.Equal(ActivityStatus.Closed, closed.Status);

        var byId = await env.ById.Handle(new GetActivityByIdQuery(created.Id), CancellationToken.None);
        Assert.Equal("# Hola", byId.MarkdownContent);
    }

    [Fact]
    public async Task Create_With_Unknown_Topic_Throws_NotFound()
    {
        var env = CreateEnv();
        await Assert.ThrowsAsync<NotFoundException>(() => env.Create.Handle(
            new CreateActivityCommand("A", "md", Guid.NewGuid(), null, null, SubmissionMode.UrlOnly),
            CancellationToken.None));
    }
}
