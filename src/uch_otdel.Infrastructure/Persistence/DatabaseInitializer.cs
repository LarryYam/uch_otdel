using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using uch_otdel.Domain.Workload;

namespace uch_otdel.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    /// <summary>
    /// Применяет миграции выбранной СУБД и, если включено, заполняет пустую базу демо-данными.
    /// Вызывается при старте приложения.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        var options = services.GetRequiredService<DatabaseOptions>();
        var factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await factory.CreateDbContextAsync(ct);

        if (options.MigrateOnStartup)
        {
            if (!db.Database.GetMigrations().Any())
            {
                throw new InvalidOperationException(
                    $"Для СУБД {options.Provider} ещё нет миграций (проект {DatabaseOptions.MigrationsAssembly(options.Provider)}). " +
                    "Создайте их командой из CLAUDE.md, раздел «Миграции».");
            }
            await db.Database.MigrateAsync(ct);
        }

        if (options.SeedDemoData)
        {
            await DemoDataSeeder.SeedAsync(db, services.GetRequiredService<WorkloadNorms>(), ct);
        }
    }
}
