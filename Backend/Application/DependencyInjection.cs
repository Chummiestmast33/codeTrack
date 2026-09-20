using Backend.Application.Common;
using Backend.Application.Features.Attendance;
using Backend.Application.Features.Identity;
using Backend.Application.Features.Sessions;
using Backend.Application.Features.Topics;
using Backend.Application.Features.Activities;
using Backend.Application.Features.Submissions;
using Backend.Application.Features.Progress;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddTransient<IValidator<RegisterStudentCommand>, RegisterStudentValidator>();
        services.AddTransient<IValidator<LoginCommand>, LoginValidator>();
        services.AddTransient<IValidator<ResetPasswordCommand>, ResetPasswordValidator>();
        services.AddTransient<IValidator<CreateTopicCommand>, CreateTopicValidator>();
        services.AddTransient<IValidator<UpdateTopicCommand>, UpdateTopicValidator>();
        services.AddTransient<IValidator<CreateSessionCommand>, CreateSessionValidator>();
        services.AddTransient<IValidator<UpdateSessionCommand>, UpdateSessionValidator>();
        services.AddTransient<IValidator<RegisterManualAttendanceCommand>, RegisterManualAttendanceValidator>();
        services.AddTransient<IValidator<UpdateAttendanceCommand>, UpdateAttendanceValidator>();
        services.AddTransient<IValidator<RegisterAttendanceByQrCommand>, RegisterAttendanceByQrValidator>();
        services.AddTransient<IValidator<CreateActivityCommand>, CreateActivityValidator>();
        services.AddTransient<IValidator<UpdateActivityCommand>, UpdateActivityValidator>();
        services.AddTransient<IValidator<RequestUploadTicketCommand>, RequestUploadTicketValidator>();
        services.AddTransient<IValidator<SubmitActivityCommand>, SubmitActivityValidator>();
        services.AddTransient<IValidator<ReviewSubmissionCommand>, ReviewSubmissionValidator>();
        services.AddTransient<IValidator<AdjustProgressCommand>, AdjustProgressValidator>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped<SessionManager>();
        services.AddScoped<AttendanceEnricher>();
        services.AddScoped<QrTicketBuilder>();
        services.AddScoped<ActivityEnricher>();
        services.AddScoped<SubmissionEnricher>();
        services.AddScoped<ProgressEvaluator>();

        return services;
    }
}
