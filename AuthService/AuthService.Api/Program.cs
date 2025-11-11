using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Validators.Auth;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Repositories;
using AuthService.Domain.Repositories;
using AuthService.Infrastructure.Services;
using AuthService.Api.Consumers;
using AuthService.Application.Commands;
using AuthService.Application.Abstractions.Security;
using AuthService.Api.Saga.Consumers;
using FluentValidation;
var builder = WebApplication.CreateBuilder(args);

// EF Core  
builder.Services.AddDbContext<AuthDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsAssembly("AuthService.Infrastructure")));

// DI
builder.Services.AddScoped<IAuthUserRepository, AuthUserRepository>();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Handlers (đăng ký những cái bạn dùng)
builder.Services.AddScoped<RegisterAuthHandler>();
builder.Services.AddScoped<LoginAuthHandler>();
builder.Services.AddScoped<RefreshTokenHandler>();
builder.Services.AddScoped<DeleteAuthHandler>();
builder.Services.AddScoped<LogoutHandler>();
builder.Services.AddValidatorsFromAssembly(typeof(RegisterAuthValidator).Assembly);
// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<RegisterAuthConsumer>();
    x.AddConsumer<LoginAuthConsumer>();
    x.AddConsumer<RefreshTokenConsumer>();
    x.AddConsumer<LogoutConsumer>();
    x.AddConsumer<DeleteAuthConsumer>();
    x.AddConsumer<RegisterAuthSagaConsumer>();
    x.AddConsumer<AssignDefaultRoleConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitmqHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        cfg.Host(rabbitmqHost, "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("auth-service", e =>
        {
            e.ConfigureConsumer<RegisterAuthConsumer>(context);
            e.ConfigureConsumer<LoginAuthConsumer>(context);
            e.ConfigureConsumer<RefreshTokenConsumer>(context);
            e.ConfigureConsumer<LogoutConsumer>(context);
            e.ConfigureConsumer<DeleteAuthConsumer>(context);
            e.ConfigureConsumer<RegisterAuthSagaConsumer>(context);
            e.ConfigureConsumer<AssignDefaultRoleConsumer>(context);
        });
    });
});

// (Tuỳ chọn) bật JWT Bearer để bảo vệ API nội bộ (nếu có controller)
var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Secret"]!);


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
