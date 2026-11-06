using Microsoft.EntityFrameworkCore;
using uch_otdel.Application.Abstractions;
using uch_otdel.Domain.Workload;
using uch_otdel.Infrastructure;
using uch_otdel.Infrastructure.Persistence;

namespace uch_otdel.Tests;

/// <summary>
/// База для тестов сценариев: в памяти (быстро, без СУБД) или настоящая SQL Server / PostgreSQL.
/// Реализует ту же фабрику контекстов, что и приложение.
/// </summary>
public sealed class TestDatabase : IAppDbContextFactory
{
    private readonly DbContextOptions<AppDbContext> _options;

    private TestDatabase(DbContextOptions<AppDbContext> options)
    {
        _options = options;
    }

    public WorkloadNorms Norms { get; } = new();

    public AppDbContext Open() => new(_options);

    public Task<IAppDbContext> CreateAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IAppDbContext>(Open());

    /// <summary>Отдельная база в памяти для каждого теста, заполненная демо-данными.</summary>
    public static async Task<TestDatabase> InMemoryAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"uch-{Guid.NewGuid():N}")
            .Options;

        var database = new TestDatabase(options);
        await using var db = database.Open();
        await DemoDataSeeder.SeedAsync(db, database.Norms);
        return database;
    }

    /// <summary>
    /// Настоящая СУБД: пересоздаёт схему по модели и заполняет демо-данными.
    /// Миграции здесь не нужны: проверяется, что модель и запросы работают на этой СУБД.
    /// </summary>
    public static Task<TestDatabase> RealAsync(DatabaseProvider provider, string connectionString) =>
        CreateRealAsync(provider, connectionString, useMigrations: false);

    /// <summary>
    /// Настоящая СУБД, схема создаётся миграциями — так же, как при запуске приложения.
    /// </summary>
    public static Task<TestDatabase> MigratedAsync(DatabaseProvider provider, string connectionString) =>
        CreateRealAsync(provider, connectionString, useMigrations: true);

    private static async Task<TestDatabase> CreateRealAsync(DatabaseProvider provider, string connectionString, bool useMigrations)
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        DependencyInjection.Configure(builder, provider, connectionString);
        var database = new TestDatabase(builder.Options);

        // Контейнер СУБД в CI может ещё запускаться — пробуем подключиться до минуты
        var deadline = DateTime.UtcNow.AddSeconds(60);
        while (true)
        {
            try
            {
                await using var db = database.Open();
                await db.Database.EnsureDeletedAsync();
                if (useMigrations)
                {
                    await db.Database.MigrateAsync();
                }
                else
                {
                    await db.Database.EnsureCreatedAsync();
                }
                await DemoDataSeeder.SeedAsync(db, database.Norms);
                return database;
            }
            catch (Exception) when (DateTime.UtcNow < deadline)
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }
    }
}
