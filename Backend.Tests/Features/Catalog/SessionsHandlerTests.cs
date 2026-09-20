using Backend.Application.Common;
using Backend.Application.Features.Sessions;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Tests.Features.Identity;

namespace Backend.Tests.Features.Catalog;

public sealed class SessionsHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 19, 2, 0, 0, TimeSpan.Zero);
    private static readonly Guid Instructor = Guid.NewGuid();

    private sealed record Env(
        FakeTopicRepository Topics,
        FakeSessionRepository Sessions,
        CreateSessionHandler Create,
        UpdateSessionHandler Update,
        MarkSessionImpartedHandler Impart,
        CancelSessionHandler Cancel,
        GetSessionsHandler List,
        GetSessionByIdHandler ById);

    private static Env CreateEnv()
    {
        var topics = new FakeTopicRepository();
        var sessions = new FakeSessionRepository();
        var uow = new FakeUnitOfWork();
        var time = new FixedTimeProvider(Now);
        var manager = new SessionManager(sessions, topics, uow);
        return new Env(
            topics, sessions,
            new CreateSessionHandler(sessions, uow, manager, time),
            new UpdateSessionHandler(sessions, manager, time),
            new MarkSessionImpartedHandler(sessions, uow, manager, time),
            new CancelSessionHandler(sessions, uow, manager, time),
            new GetSessionsHandler(sessions, manager),
            new GetSessionByIdHandler(manager));
    }

    private static Topic SeedTopic(FakeTopicRepository topics, string name = "T1") =>
        SeedTopic(topics, name, 1);

    private static Topic SeedTopic(FakeTopicRepository topics, string name, int order)
    {
        var topic = Topic.Create(name, null, order, Now);
        topics.Seed(topic);
        return topic;
    }

    [Fact]
    public async Task Create_Links_Topics_And_Returns_Names()
    {
        var env = CreateEnv();
        var t1 = SeedTopic(env.Topics, "T1");
        var t2 = SeedTopic(env.Topics, "T2", 2);

        var dto = await env.Create.Handle(
            new CreateSessionCommand("S1", "desc", Now.AddDays(1), [t1.Id, t2.Id], Instructor),
            CancellationToken.None);

        Assert.Equal("S1", dto.Title);
        Assert.Equal(SessionStatus.Planned, dto.Status);
        Assert.Equal(["T1", "T2"], dto.Topics.Select(t => t.Name).OrderBy(n => n));
    }

    [Fact]
    public async Task Create_With_Unknown_Topic_Throws_NotFound()
    {
        var env = CreateEnv();
        await Assert.ThrowsAsync<NotFoundException>(() => env.Create.Handle(
            new CreateSessionCommand("S", null, Now, [Guid.NewGuid()], Instructor),
            CancellationToken.None));
    }

    [Fact]
    public async Task Update_Replaces_Topics_And_Lifecycle_Works()
    {
        var env = CreateEnv();
        var t1 = SeedTopic(env.Topics, "T1");
        var t2 = SeedTopic(env.Topics, "T2", 2);
        var created = await env.Create.Handle(
            new CreateSessionCommand("S", null, Now, [t1.Id], Instructor), CancellationToken.None);

        var updated = await env.Update.Handle(
            new UpdateSessionCommand(created.Id, "S2", null, Now.AddDays(2), [t2.Id]), CancellationToken.None);
        Assert.Equal("S2", updated.Title);
        Assert.Equal([t2.Id], updated.Topics.Select(t => t.Id));

        var imparted = await env.Impart.Handle(new MarkSessionImpartedCommand(created.Id), CancellationToken.None);
        Assert.Equal(SessionStatus.Imparted, imparted.Status);

        var cancelled = await env.Cancel.Handle(new CancelSessionCommand(created.Id), CancellationToken.None);
        Assert.Equal(SessionStatus.Cancelled, cancelled.Status);

        var listed = await env.List.Handle(new GetSessionsQuery(), CancellationToken.None);
        Assert.Single(listed);
    }

    [Fact]
    public void Validator_Requires_Title_And_Topics()
    {
        var validator = new CreateSessionValidator();
        Assert.False(validator.Validate(new CreateSessionCommand("  ", null, Now, [Guid.NewGuid()], Instructor)).IsValid);
        Assert.False(validator.Validate(new CreateSessionCommand("S", null, Now, [], Instructor)).IsValid);
    }
}
