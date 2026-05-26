using ExamEngine.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuizManagement.Infrastructure.Persistence;
using StackExchange.Redis;
using Testcontainers.MsSql;
using Testcontainers.Redis;

namespace Quizora.IntegrationTests.Fixtures;

public sealed class QuizoraWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _mssql = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder().Build();

    public async Task InitializeAsync()
    {
        await _mssql.StartAsync();
        await _redis.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<IdentityDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlServer(_mssql.GetConnectionString()));

            var qmDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<QuizManagementDbContext>));
            if (qmDescriptor is not null) services.Remove(qmDescriptor);

            services.AddDbContext<QuizManagementDbContext>(options =>
                options.UseSqlServer(_mssql.GetConnectionString()));

            var eeDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ExamEngineDbContext>));
            if (eeDescriptor is not null) services.Remove(eeDescriptor);

            services.AddDbContext<ExamEngineDbContext>(options =>
                options.UseSqlServer(_mssql.GetConnectionString()));

            var redisDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IConnectionMultiplexer));
            if (redisDescriptor is not null) services.Remove(redisDescriptor);

            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(_redis.GetConnectionString()));
        });

        builder.UseSetting("Jwt:PublicKeyPem", TestKeys.PublicKeyPem);
        builder.UseSetting("Jwt:PrivateKeyPem", TestKeys.PrivateKeyPem);
        builder.UseSetting("Jwt:Issuer", "Quizora");
        builder.UseSetting("Jwt:Audience", "Quizora.Client");
        builder.UseSetting("Frontend:Url", "http://localhost:5173");
        builder.UseSetting("OpenTelemetry:Endpoint", "http://localhost:4317");
        builder.UseEnvironment("Testing");
    }

    public async Task<HttpClient> GetClientWithMigratedDbAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await db.Database.MigrateAsync();
        var qmDb = scope.ServiceProvider.GetRequiredService<QuizManagementDbContext>();
        await qmDb.Database.MigrateAsync();
        var eeDb = scope.ServiceProvider.GetRequiredService<ExamEngineDbContext>();
        await eeDb.Database.MigrateAsync();
        return CreateClient();
    }

    public new async Task DisposeAsync()
    {
        await _mssql.DisposeAsync();
        await _redis.DisposeAsync();
        await base.DisposeAsync();
    }
}
