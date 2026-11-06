using Microsoft.EntityFrameworkCore;
using uch_otdel.Infrastructure.Persistence;
using Xunit.Abstractions;

namespace uch_otdel.Tests.Application;

/// <summary>
/// Те же сценарии на настоящих SQL Server и PostgreSQL. Строки подключения берутся из переменных окружения
/// UCH_TEST_SQLSERVER и UCH_TEST_POSTGRES (в CI их задаёт .github/workflows/ci.yml).
/// Если переменная не задана, тест для этой СУБД пропускается с сообщением в выводе.
/// Внимание: тест удаляет и пересоздаёт базу — указывайте только отдельную тестовую базу.
/// </summary>
[Trait("Category", "Integration")]
public class DatabaseProviderTests
{
    private readonly ITestOutputHelper _output;

    public DatabaseProviderTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public static TheoryData<DatabaseProvider> Providers => new()
    {
        DatabaseProvider.SqlServer,
        DatabaseProvider.PostgreSql
    };

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task All_scenarios_work_on_real_database(DatabaseProvider provider)
    {
        var variable = provider == DatabaseProvider.SqlServer ? "UCH_TEST_SQLSERVER" : "UCH_TEST_POSTGRES";
        var connectionString = Environment.GetEnvironmentVariable(variable);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _output.WriteLine($"Пропущено: не задана переменная {variable}.");
            return;
        }

        // Каждый сценарий — на свежей базе, чтобы они не влияли друг на друга
        var scenarios = new (string Name, Func<TestDatabase, Task> Run)[]
        {
            (nameof(Scenarios.TeacherLoadsAndStatuses), Scenarios.TeacherLoadsAndStatuses),
            (nameof(Scenarios.FiltersAndCaseInsensitiveSearch), Scenarios.FiltersAndCaseInsensitiveSearch),
            (nameof(Scenarios.UpdateRecalculatesAndValidates), Scenarios.UpdateRecalculatesAndValidates),
            (nameof(Scenarios.BulkOperations), Scenarios.BulkOperations),
            (nameof(Scenarios.PivotEntries), Scenarios.PivotEntries),
            (nameof(Scenarios.PlanLinks), Scenarios.PlanLinks)
        };

        foreach (var (name, run) in scenarios)
        {
            _output.WriteLine($"{provider}: {name}");
            await run(await TestDatabase.RealAsync(provider, connectionString));
        }
    }

    /// <summary>
    /// Миграции этой СУБД создают рабочую базу и не отстают от модели.
    /// Падает, если изменили сущности или Configurations и забыли добавить миграцию.
    /// </summary>
    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Migrations_create_working_database_and_match_model(DatabaseProvider provider)
    {
        var variable = provider == DatabaseProvider.SqlServer ? "UCH_TEST_SQLSERVER" : "UCH_TEST_POSTGRES";
        var connectionString = Environment.GetEnvironmentVariable(variable);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _output.WriteLine($"Пропущено: не задана переменная {variable}.");
            return;
        }

        var database = await TestDatabase.MigratedAsync(provider, connectionString);

        await using (var db = database.Open())
        {
            Assert.Empty(await db.Database.GetPendingMigrationsAsync());
            Assert.False(db.Database.HasPendingModelChanges(),
                "Модель изменилась, а миграции нет. Создайте миграцию для обеих СУБД (CLAUDE.md, «Миграции»).");
        }

        await Scenarios.TeacherLoadsAndStatuses(database);
        await Scenarios.PlanLinks(database);
    }
}
