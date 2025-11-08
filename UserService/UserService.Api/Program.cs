using MassTransit;
using Microsoft.EntityFrameworkCore;
using UserService.Api.Consumers;
using UserService.Infrastructure.Data;
using Validators.Users;
using FluentValidation;
using UserService.Application.Users.Commands;
using UserService.Application.Users.Queries;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Repositories;
var builder = WebApplication.CreateBuilder(args);

// ======================
// Database (PostgreSQL)
// ======================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<CreateUserHandler>();
builder.Services.AddScoped<DeleteUserHandler>();
builder.Services.AddScoped<GetUserHandler>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<GetUserRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<DeleteUserRequestValidator>();
builder.Services.AddScoped<IUserRepository, UserRepository>();


// ======================
// MassTransit + RabbitMQ
// ======================
builder.Services.AddMassTransit(x =>
{
    // Đăng ký consumer
    x.AddConsumer<GetUserConsumer>();
    x.AddConsumer<CreateUserConsumer>();
    x.AddConsumer<DeleteUserConsumer>();



    // ========================
    // RabbitMQ configuration
    // ========================
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });


        cfg.ReceiveEndpoint("user-service-queue", e =>
        {
            e.ConfigureConsumer<GetUserConsumer>(context);
            e.ConfigureConsumer<CreateUserConsumer>(context);
            e.ConfigureConsumer<DeleteUserConsumer>(context);
        });
    });
});

// ======================
// API + Swagger
// ======================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ======================
// Middleware
// ======================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
