using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MailService.Application.Abstractions;
using MailService.Infrastructure.Services;
using MassTransit;
using MailService.Api.Consumers;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Đọc Resend API Key từ cấu hình
var resendApiKey = builder.Configuration["Resend:ApiKey"] ?? "YOUR_RESEND_API_KEY";
var fromEmail = builder.Configuration["Resend:FromEmail"] ?? "noreply@example.com";
builder.Services.AddSingleton<IMailSender>(new ResendMailSender(resendApiKey, fromEmail));

// Redis connection
var redisConnection = builder.Configuration["Redis:Connection"] ?? "redis:6379";
var options = ConfigurationOptions.Parse(redisConnection);
options.AbortOnConnectFail = false;  // Retry instead of failing
var redis = ConnectionMultiplexer.Connect(options);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddSingleton<ITokenService, RedisTokenService>();

// MassTransit cấu hình RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SendMailConsumer>();
    x.AddConsumer<SendConfirmationEmailConsumer>();
    x.AddConsumer<VerifyEmailConsumer>();
    x.AddConsumer<CheckEmailTokenConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("mail-service", e =>
        {
            e.ConfigureConsumer<SendMailConsumer>(context);
            e.ConfigureConsumer<SendConfirmationEmailConsumer>(context);
            e.ConfigureConsumer<VerifyEmailConsumer>(context);
            e.ConfigureConsumer<CheckEmailTokenConsumer>(context);
        });
    });
});

var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.MapGet("/health", () => "OK").WithName("Health").WithOpenApi();

await app.RunAsync();