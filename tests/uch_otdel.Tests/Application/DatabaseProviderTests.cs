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
}
