namespace uch_otdel.Infrastructure.Persistence;

public enum DatabaseProvider
{
    SqlServer,
    PostgreSql
}

/// <summary>Настройки базы данных (секция "Database" в appsettings.json).</summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public DatabaseProvider Provider { get; set; } = DatabaseProvider.SqlServer;

    public string ConnectionString { get; set; } = "";

    /// <summary>Применять миграции при запуске приложения (удобно для установки у клиента).</summary>
    public bool MigrateOnStartup { get; set; } = true;

    /// <summary>Заполнить пустую базу демонстрационными данными.</summary>
    public bool SeedDemoData { get; set; }

    /// <summary>Имя сборки с миграциями для выбранной СУБД.</summary>
    public static string MigrationsAssembly(DatabaseProvider provider) => provider switch
    {
        DatabaseProvider.SqlServer => "uch_otdel.Migrations.SqlServer",
        DatabaseProvider.PostgreSql => "uch_otdel.Migrations.PostgreSql",
        _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "Неизвестная СУБД")
    };
}
