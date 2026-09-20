using Backend.Application.Common;
using Backend.Application.Features.Topics;
using Backend.Domain.Entities;
using Backend.Tests.Features.Identity;

namespace Backend.Tests.Features.Catalog;

public sealed class TopicsHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 19, 2, 0, 0, TimeSpan.Zero);

    private static (CreateTopicHandler Create, UpdateTopicHandler Update, ActivateTopicHandler Activate,
        DeactivateTopicHandler Deactivate, GetTopicsHandler List, GetTopicByIdHandler ById, FakeTopicRepository Topics)
        CreateEnv()
    {
        var topics = new FakeTopicRepository();
        var uow = new FakeUnitOfWork();
        var time = new FixedTimeProvider(Now);
        return (
            new CreateTopicHandler(topics, uow, time),
            new UpdateTopicHandler(topics, uow, time),
            new ActivateTopicHandler(topics, uow, time),
            new DeactivateTopicHandler(topics, uow, time),
            new GetTopicsHandler(topics),
            new GetTopicByIdHandler(topics),
            topics);
    }

    [Fact]
    public async Task Create_And_List_Ordered()
    {
        var env = CreateEnv();
        await env.Create.Handle(new CreateTopicCommand("B", null, 2), CancellationToken.None);
        await env.Create.Handle(new CreateTopicCommand("A", "desc", 1), CancellationToken.None);

        var list = await env.List.Handle(new GetTopicsQuery(), CancellationToken.None);

        Assert.Equal(["A", "B"], list.Select(t => t.Name));
    }

    [Fact]
    public async Task Update_Rename_Reorder_And_Toggle_Active()
    {
        var env = CreateEnv();
        var created = await env.Create.Handle(new CreateTopicCommand("Old", null, 5), CancellationToken.None);

        var updated = await env.Update.Handle(new UpdateTopicCommand(created.Id, "New", "d", 1), CancellationToken.None);
        Assert.Equal(("New", 1), (updated.Name, updated.OrderNumber));

        var off = await env.Deactivate.Handle(new DeactivateTopicCommand(created.Id), CancellationToken.None);
        Assert.False(off.IsActive);

        var on = await env.Activate.Handle(new ActivateTopicCommand(created.Id), CancellationToken.None);
        Assert.True(on.IsActive);

        var byId = await env.ById.Handle(new GetTopicByIdQuery(created.Id), CancellationToken.None);
        Assert.Equal("New", byId.Name);
    }

    [Fact]
    public async Task Unknown_Topic_Throws_NotFound()
    {
        var env = CreateEnv();
        var id = Guid.NewGuid();
        await Assert.ThrowsAsync<NotFoundException>(() => env.ById.Handle(new GetTopicByIdQuery(id), CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => env.Update.Handle(new UpdateTopicCommand(id, "x", null, 0), CancellationToken.None));
    }

    [Fact]
    public void Validator_Rejects_Empty_Name()
    {
        var validator = new CreateTopicValidator();
        Assert.False(validator.Validate(new CreateTopicCommand("  ", null, 0)).IsValid);
    }
}
