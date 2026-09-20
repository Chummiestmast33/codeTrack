using Backend.Api;
using Backend.Application.Abstractions;
using Backend.Application.Common;
using Backend.Application.Features.Attendance;
using Backend.Application.Features.Identity;
using Backend.Application.Features.Sessions;
using Backend.Application.Features.Topics;
using Backend.Infrastructure;
using Backend.Middleware;
using Backend.OpenApi;
using FluentValidation;
using MediatR;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "CodeTrack API";
        document.Info.Description = "Activities and progress tracking system for students and teachers.";
        return Task.CompletedTask;
    });
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection(AuthOptions.SectionName));
builder.Services.AddFrontendCors(builder.Configuration);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddTransient<IValidator<RegisterStudentCommand>, RegisterStudentValidator>();
builder.Services.AddTransient<IValidator<LoginCommand>, LoginValidator>();
builder.Services.AddTransient<IValidator<ResetPasswordCommand>, ResetPasswordValidator>();
builder.Services.AddTransient<IValidator<CreateTopicCommand>, CreateTopicValidator>();
builder.Services.AddTransient<IValidator<UpdateTopicCommand>, UpdateTopicValidator>();
builder.Services.AddTransient<IValidator<CreateSessionCommand>, CreateSessionValidator>();
builder.Services.AddTransient<IValidator<UpdateSessionCommand>, UpdateSessionValidator>();
builder.Services.AddTransient<IValidator<RegisterManualAttendanceCommand>, RegisterManualAttendanceValidator>();
builder.Services.AddTransient<IValidator<UpdateAttendanceCommand>, UpdateAttendanceValidator>();
builder.Services.AddTransient<IValidator<RegisterAttendanceByQrCommand>, RegisterAttendanceByQrValidator>();
builder.Services.AddScoped<SessionManager>();
builder.Services.AddScoped<AttendanceEnricher>();
builder.Services.AddScoped<QrTicketBuilder>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseGlobalExceptionHandling();
app.UseCors(CorsOptions.PolicyName);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
