using System;
using System.Text;
using MassTransit;
using Contracts.Users;
using Contracts.Auth;
using Contracts.Mail;
using Contracts.Saga.Auth;
using Contracts.Url;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ApiGateway.Middleware;
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

// ✅ Config Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// ✅ Config JWT Authentication
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var jwtSecret = builder.Configuration["Jwt:Secret"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret!))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
});

// ✅ Config MassTransit với RabbitMQ
builder.Services.AddMassTransit(x =>
{
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

// ✅ Register RequestClients for Auth (login, logout, refresh token)
builder.Services.AddScoped(provider =>
    provider.GetRequiredService<IBus>().CreateRequestClient<LoginAuthRequest>());
builder.Services.AddScoped(provider =>
    provider.GetRequiredService<IBus>().CreateRequestClient<RefreshTokenRequest>());
builder.Services.AddScoped(provider =>
    provider.GetRequiredService<IBus>().CreateRequestClient<LogoutRequest>());

// ✅ Register RequestClients for Auth Saga (register, verify email)
builder.Services.AddScoped(provider =>
    provider.GetRequiredService<IBus>().CreateRequestClient<RegisterRequestedEvent>(
        timeout: TimeSpan.FromSeconds(30)));
builder.Services.AddScoped(provider =>
    provider.GetRequiredService<IBus>().CreateRequestClient<Contracts.Saga.VerifyEmailRequestedEvent>());

// ✅ Register RequestClient for Admin Dashboard Saga
builder.Services.AddScoped(provider =>
    provider.GetRequiredService<IBus>().CreateRequestClient<Contracts.Saga.GetUserWithUrlsRequest>(
        timeout: TimeSpan.FromSeconds(30)));
builder.Services.AddScoped(provider =>
    provider.GetRequiredService<IBus>().CreateRequestClient<Contracts.Saga.GetAllUsersWithUrlsRequest>(
        timeout: TimeSpan.FromMinutes(2)));
builder.Services.AddScoped(provider =>
    provider.GetRequiredService<IBus>().CreateRequestClient<Contracts.Saga.DeleteUserSagaRequest>(
        timeout: TimeSpan.FromSeconds(30)));

// ✅ Register RequestClients for URL Service
builder.Services.AddScoped(provider =>
    provider.GetRequiredService<IBus>().CreateRequestClient<CreateShortUrlRequest>());
builder.Services.AddScoped(provider =>
    provider.GetRequiredService<IBus>().CreateRequestClient<ResolveShortUrlRequest>());
builder.Services.AddScoped(provider =>
    provider.GetRequiredService<IBus>().CreateRequestClient<GetListShortUrlsRequest>());
builder.Services.AddScoped(provider =>
    provider.GetRequiredService<IBus>().CreateRequestClient<DeleteShortUrlRequest>());

// ✅ Register RequestClient for User Service (get userId from authId)
builder.Services.AddScoped(provider =>
    provider.GetRequiredService<IBus>().CreateRequestClient<GetUserByAuthIdRequest>());

// ✅ Config CORS - Allow all origins for development/testing
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below. Example: \"Bearer eyJhbGci...\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "bearer",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// ✅ Configure Forwarded Headers (Trust Cloudflare proxy)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor 
                     | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

// ✅ Security Headers Middleware (for Cloudflare/Browser security)
app.UseMiddleware<SecurityHeadersMiddleware>();

// Global Exception Handling Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// ✅ Enable CORS (MUST be before Authentication/Authorization)
app.UseCors("AllowAll");

// ✅ Enable Rate Limiting (MUST be before Authentication/Authorization)
app.UseIpRateLimiting();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run("http://0.0.0.0:8080");