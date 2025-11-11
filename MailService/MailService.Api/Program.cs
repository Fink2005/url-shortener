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
var resendApiKey = builder.Configuration["Resend:ApiKey"] ?? "re_KsNYvuia_7tbTiPLHkMvuWeDocdokLVLJ";
var fromEmail = builder.Configuration["Resend:FromEmail"] ?? "no-reply@url-shortener.site";

// HttpClient cho Resend with factory
builder.Services.AddHttpClient<IMailSender, ResendMailSender>()
    .ConfigureHttpClient(client =>
    {
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {resendApiKey}");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    })
    .AddTypedClient<IMailSender>((httpClient, sp) => new ResendMailSender(httpClient, fromEmail));

// Redis connection
var redisConnection = builder.Configuration["Redis:Connection"] ?? "localhost:6379";
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
    x.AddConsumer<VerifyMailSagaConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitmqHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        cfg.Host(rabbitmqHost, "/", h =>
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
            e.ConfigureConsumer<VerifyMailSagaConsumer>(context);
        });
    });
});

var app = builder.Build();




app.UseRouting();
app.MapControllers();
app.MapGet("/health", () => "OK").WithName("Health").WithOpenApi();

await app.RunAsync();