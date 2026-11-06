using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace uch_otdel.Infrastructure.Persistence;

/// <summary>
/// Используется только командами <c>dotnet ef</c> при создании миграций.
/// СУБД передаётся после двойного дефиса: <c>-- --provider PostgreSql</c>.
/// Подключение к базе для создания миграций не нужно, поэтому строка подключения — заглушка.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var provider = DatabaseProvider.SqlServer;
        var index = Array.FindIndex(args, a => string.Equals(a, "--provider", StringComparison.OrdinalIgnoreCase));
        if (index >= 0 && index + 1 < args.Length)
        {
            if (!Enum.TryParse(args[index + 1], ignoreCase: true, out provider))
            {
                throw new ArgumentException(
                    $"Неизвестная СУБД '{args[index + 1]}'. Допустимо: {string.Join(", ", Enum.GetNames<DatabaseProvider>())}.");
            }
        }

        var connectionString = provider switch
        {
            DatabaseProvider.PostgreSql => "Host=localhost;Database=uch_otdel_design;Username=postgres",
            _ => "Server=localhost;Database=uch_otdel_design;Trusted_Connection=True;TrustServerCertificate=True"
        };

        var builder = new DbContextOptionsBuilder<AppDbContext>();
        DependencyInjection.Configure(builder, provider, connectionString);
        return new AppDbContext(builder.Options);
    }
}
