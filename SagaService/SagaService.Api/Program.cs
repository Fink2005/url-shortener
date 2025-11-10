using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using SagaService.Infrastructure.Data;
using SagaService.Domain.States;

var builder = WebApplication.CreateBuilder(args);

// ĐK DbContext cho EF/Migrations
builder.Services.AddDbContext<SagaStateDbContext>(opt =>
    opt.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.MigrationsAssembly("SagaService.Infrastructure")
    )
);

builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<UserOnboardingStateMachine, UserOnboardingState>()
        .EntityFrameworkRepository(r =>
        {
            r.ExistingDbContext<SagaStateDbContext>();
            r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
            r.LockStatementProvider = new PostgresLockStatementProvider();
        });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();
app.Run();
