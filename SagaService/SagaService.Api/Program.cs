using MassTransit;
using SagaService.Domain.States;
using SagaService.Api.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    // Use InMemory repository for all sagas (no database needed)
    x.AddSagaStateMachine<UserOnboardingStateMachine, UserOnboardingState>()
        .InMemoryRepository();

    // Register Consumers
    x.AddConsumer<RegisterRequestedConsumer>();
    x.AddConsumer<GetUserWithUrlsConsumer>();
    x.AddConsumer<GetAllUsersWithUrlsConsumer>();
    x.AddConsumer<DeleteUserSagaConsumer>();

    // Register Request Clients
    // Note: SendConfirmationEmailRequest removed - Saga handles email via SendConfirmationEmailCommand
    x.AddRequestClient<Contracts.Users.GetUserRequest>(TimeSpan.FromSeconds(10));
    x.AddRequestClient<Contracts.Users.GetListUsersRequest>(TimeSpan.FromSeconds(30));
    x.AddRequestClient<Contracts.Users.DeleteUserRequest>(TimeSpan.FromSeconds(10));
    x.AddRequestClient<Contracts.Auth.GetAuthByIdRequest>(TimeSpan.FromSeconds(10));
    x.AddRequestClient<Contracts.Auth.GetAuthsByIdsRequest>(TimeSpan.FromSeconds(15));
    x.AddRequestClient<Contracts.Auth.DeleteAuthRequest>(TimeSpan.FromSeconds(10));
    x.AddRequestClient<Contracts.Url.GetUrlsByUserRequest>(TimeSpan.FromSeconds(10));
    x.AddRequestClient<Contracts.Url.GetUrlsByUserIdsRequest>(TimeSpan.FromSeconds(15));

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
