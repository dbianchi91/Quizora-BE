using System.Security.Cryptography;
using Quizora.SharedKernel;
using Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using AITutor.Infrastructure;
using Analytics.Infrastructure;
using ExamEngine.Infrastructure;
using StackExchange.Redis;
using Quizora.API.Infrastructure;
using Quizora.API.Middleware;
using QuizManagement.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, services, config) =>
    config.ReadFrom.Configuration(ctx.Configuration)
          .Enrich.FromLogContext()
          .Enrich.WithEnvironmentName()
          .Enrich.WithThreadId());

// OpenTelemetry
var otelEndpoint = builder.Configuration["OpenTelemetry:Endpoint"]!;
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("Quizora.API"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri(otelEndpoint)))
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri(otelEndpoint)));

// JWT Auth (RS256)
var publicKeyPem = builder.Configuration["Jwt:PublicKeyPem"]!;
var rsa = RSA.Create();
if (!string.IsNullOrWhiteSpace(publicKeyPem))
    rsa.ImportFromPem(publicKeyPem);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(rsa),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Creator", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim("is_creator", "true") ||
            ctx.User.HasClaim("is_admin", "true")));
    options.AddPolicy("Admin", policy =>
        policy.RequireClaim("is_admin", "true"));
});

// CORS
builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(p =>
        p.WithOrigins(builder.Configuration["Frontend:Url"]!)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()));

// Health checks
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!);

// OpenAPI
builder.Services.AddOpenApi();

// Identity module
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddQuizManagementModule(builder.Configuration);

var redisConn = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConn));
builder.Services.AddExamEngineModule(builder.Configuration);
builder.Services.AddAITutorModule(builder.Configuration);
builder.Services.AddAnalyticsModule(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseSerilogRequestLogging();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapAllModuleEndpoints();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.Run();

public partial class Program { }
