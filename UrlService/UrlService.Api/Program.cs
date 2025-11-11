using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using UrlService.Domain.Repositories;
using UrlService.Infrastructure.Data;
using Validators.Url;
using UrlService.Infrastructure.Repositories;
using MassTransit;
using UrlService.Application.Commands;
using UrlService.Api.Consumers;
var builder = WebApplication.CreateBuilder(args);

// EF Core (Postgres)
builder.Services.AddDbContext<UrlDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// DI
builder.Services.AddScoped<IShortUrlRepository, ShortUrlRepository>();
builder.Services.AddScoped<CreateShortUrlHandler>();
builder.Services.AddScoped<ResolveShortUrlHandler>();
builder.Services.AddScoped<GetListShortUrlsHandler>();
builder.Services.AddScoped<DisableShortUrlHandler>();
builder.Services.AddScoped<DeleteShortUrlHandler>();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(CreateShortUrlValidator).Assembly);


builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CreateShortUrlConsumer>();
    x.AddConsumer<ResolveShortUrlConsumer>();
    x.AddConsumer<DisableShortUrlConsumer>();
    x.AddConsumer<GetListShortUrlConsumer>();
    x.AddConsumer<DeleteShortUrlConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("auth-service-queue", e =>
        {
            e.ConfigureConsumer<CreateShortUrlConsumer>(context);
            e.ConfigureConsumer<ResolveShortUrlConsumer>(context);
            e.ConfigureConsumer<DisableShortUrlConsumer>(context);
            e.ConfigureConsumer<GetListShortUrlConsumer>(context);
            e.ConfigureConsumer<DeleteShortUrlConsumer>(context);
        });
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
