using MassTransit;
using Contracts.Users;


var builder = WebApplication.CreateBuilder(args);

// ✅ Config MassTransit với RabbitMQ
builder.Services.AddMassTransit(x =>
{
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ✅ CHỈ GỌI AddSwaggerGen MỘT LẦN
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Url Shortener API Gateway", // 👈 đổi tên title ở đây
        Version = "v1",
        Description = "Gateway service that routes and orchestrates user-related requests.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Url Shortener Dev Team",
            Email = "dev@url-shortener.io.vn",
            Url = new Uri("https://url-shortener.io.vn")
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // 👇 bạn có thể đổi tên hiển thị ở đây nữa nếu muốn
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Url Shortener API Gateway v1");
        c.DocumentTitle = "Url Shortener Gateway Docs"; // Tiêu đề tab trình duyệt
    });
}

app.MapControllers();

app.Run("http://0.0.0.0:5050");
