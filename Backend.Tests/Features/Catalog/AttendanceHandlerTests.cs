using Backend.Application.Abstractions;
using Backend.Application.Common;
using Backend.Application.Features.Attendance;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Tests.Features.Identity;
using Microsoft.Extensions.Options;

namespace Backend.Tests.Features.Catalog;

public sealed class AttendanceHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 19, 2, 0, 0, TimeSpan.Zero);

    private sealed record Env(
        FakeUserRepository Users,
        FakeSessionRepository Sessions,
        FakeAttendanceRepository Attendance,
        FakeQrTokenRepository Qr,
        RegisterManualAttendanceHandler Manual,
        UpdateAttendanceHandler Update,
        RegisterAttendanceByQrHandler ByQr,
        GenerateQrTokenHandler Generate,
        GetQrHandler GetQr,
        GetAttendanceBySessionHandler BySession);

    private static Env CreateEnv(int qrExpiryMinutes = 120)
    {
        var users = new FakeUserRepository();
        var sessions = new FakeSessionRepository();
        var attendance = new FakeAttendanceRepository();
        var qr = new FakeQrTokenRepository();
        var uow = new FakeUnitOfWork();
        var time = new FixedTimeProvider(Now);
        var enricher = new AttendanceEnricher();
        var tickets = new QrTicketBuilder(new OptionsWrapper<QrOptions>(
            new QrOptions { ExpiryMinutes = qrExpiryMinutes, FrontendBaseUrl = "http://localhost:5173" }));
        return new Env(
            users, sessions, attendance, qr,
            new RegisterManualAttendanceHandler(attendance, users, uow, enricher, time),
            new UpdateAttendanceHandler(attendance, users, uow, enricher, time),
            new RegisterAttendanceByQrHandler(qr, sessions, attendance, users, uow, enricher, time),
            new GenerateQrTokenHandler(qr, sessions, uow, tickets, time),
            new GetQrHandler(qr, tickets, time),
            new GetAttendanceBySessionHandler(attendance, sessions, users, enricher));
    }

    private static User ApprovedStudent(FakeUserRepository users, string control = "q100")
    {
        var user = User.RegisterStudent(control, "N", control + "@x.com", "h", Now);
        user.Approve(Now);
        users.Seed(user);
        return user;
    }

    private static Session PlannedSession(FakeSessionRepository sessions)
    {
        var session = Session.Create("S", null, Now.AddDays(1), Guid.NewGuid(), Now);
        sessions.Seed(session);
        return session;
    }

    [Fact]
    public async Task Qr_Register_Marks_Present_With_Student_Data()
    {
        var env = CreateEnv();
        var user = ApprovedStudent(env.Users);
        var session = PlannedSession(env.Sessions);
        var ticket = await env.Generate.Handle(new GenerateQrTokenCommand(session.Id), CancellationToken.None);

        Assert.StartsWith("http://localhost:5173/app/attend/", ticket.AttendUrl);
        Assert.Equal(Now.AddMinutes(120), ticket.ExpiresAt);

        var dto = await env.ByQr.Handle(new RegisterAttendanceByQrCommand(ticket.Token, user.Id), CancellationToken.None);

        Assert.Equal(AttendanceStatus.Present, dto.Status);
        Assert.Equal("q100", dto.ControlNumber);
    }

    [Fact]
    public async Task Qr_Register_Duplicate_Throws_Conflict()
    {
        var env = CreateEnv();
        var user = ApprovedStudent(env.Users);
        var session = PlannedSession(env.Sessions);
        var ticket = await env.Generate.Handle(new GenerateQrTokenCommand(session.Id), CancellationToken.None);

        await env.ByQr.Handle(new RegisterAttendanceByQrCommand(ticket.Token, user.Id), CancellationToken.None);
        await Assert.ThrowsAsync<ConflictException>(() =>
            env.ByQr.Handle(new RegisterAttendanceByQrCommand(ticket.Token, user.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Qr_Expired_Throws_Gone_And_Unknown_Throws_NotFound()
    {
        var env = CreateEnv(qrExpiryMinutes: 120);
        var user = ApprovedStudent(env.Users);
        var session = PlannedSession(env.Sessions);

        var expired = QrToken.Create(session.Id, "old", Now.AddMinutes(-1), Now.AddHours(-2));
        await env.Qr.AddAsync(expired, CancellationToken.None);

        await Assert.ThrowsAsync<GoneException>(() =>
            env.ByQr.Handle(new RegisterAttendanceByQrCommand("old", user.Id), CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            env.ByQr.Handle(new RegisterAttendanceByQrCommand("missing", user.Id), CancellationToken.None));
        await Assert.ThrowsAsync<GoneException>(() =>
            env.GetQr.Handle(new GetQrQuery(session.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Qr_Pending_User_Throws_Forbidden_And_Cancelled_Session_Throws_Conflict()
    {
        var env = CreateEnv();
        var pending = User.RegisterStudent("q101", "N", "q101@x.com", "h", Now);
        env.Users.Seed(pending);
        var session = PlannedSession(env.Sessions);
        var ticket = await env.Generate.Handle(new GenerateQrTokenCommand(session.Id), CancellationToken.None);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            env.ByQr.Handle(new RegisterAttendanceByQrCommand(ticket.Token, pending.Id), CancellationToken.None));

        session.Cancel(Now);
        var user = ApprovedStudent(env.Users, "q102");
        await Assert.ThrowsAsync<ConflictException>(() =>
            env.ByQr.Handle(new RegisterAttendanceByQrCommand(ticket.Token, user.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Manual_Register_Update_And_List()
    {
        var env = CreateEnv();
        var admin = User.RegisterAdministrator("a1", "A", "a@x.com", "h", Now);
        admin.Approve(Now);
        env.Users.Seed(admin);
        var user = ApprovedStudent(env.Users);
        var session = PlannedSession(env.Sessions);

        var dto = await env.Manual.Handle(
            new RegisterManualAttendanceCommand(session.Id, user.Id, AttendanceStatus.Late, "traffic", admin.Id),
            CancellationToken.None);
        Assert.Equal(AttendanceStatus.Late, dto.Status);

        await Assert.ThrowsAsync<ConflictException>(() => env.Manual.Handle(
            new RegisterManualAttendanceCommand(session.Id, user.Id, AttendanceStatus.Present, null, admin.Id),
            CancellationToken.None));

        var updated = await env.Update.Handle(
            new UpdateAttendanceCommand(dto.Id, AttendanceStatus.Present, null), CancellationToken.None);
        Assert.Equal(AttendanceStatus.Present, updated.Status);

        var list = await env.BySession.Handle(new GetAttendanceBySessionQuery(session.Id), CancellationToken.None);
        var single = Assert.Single(list);
        Assert.Equal("q100", single.ControlNumber);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            env.BySession.Handle(new GetAttendanceBySessionQuery(Guid.NewGuid()), CancellationToken.None));
    }
}
