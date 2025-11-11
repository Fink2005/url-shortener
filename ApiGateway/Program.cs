using MassTransit;
using Contracts.Users;
using Contracts.Auth;

var builder = WebApplication.CreateBuilder(args);

// ✅ Config MassTransit với RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

// ✅ Register RequestClients for auth requests
builder.Services.AddScoped(provider =>
    provider.GetRequiredService<IBus>().CreateRequestClient<RegisterAuthRequest>());
builder.Services.AddScoped(provider =>
    provider.GetRequiredService<IBus>().CreateRequestClient<LoginAuthRequest>());
builder.Services.AddScoped(provider =>
    provider.GetRequiredService<IBus>().CreateRequestClient<RefreshTokenRequest>());
builder.Services.AddScoped(provider =>
    provider.GetRequiredService<IBus>().CreateRequestClient<LogoutRequest>());
builder.Services.AddScoped(provider =>
    provider.GetRequiredService<IBus>().CreateRequestClient<DeleteAuthRequest>());

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

app.Run("http://0.0.0.0:8080");