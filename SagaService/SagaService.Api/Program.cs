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

    // Register Consumers
    x.AddConsumer<RegisterSagaConsumer>();

    // Register Request Clients
    x.AddRequestClient<SendConfirmationEmailCommand>(TimeSpan.FromSeconds(30));
    x.AddRequestClient<VerifyAuthUserEmailCommand>(TimeSpan.FromSeconds(30));

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitmqHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        cfg.Host(rabbitmqHost, "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
