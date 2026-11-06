using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using uch_otdel.Application.Abstractions;
using uch_otdel.Infrastructure.Persistence;

namespace uch_otdel.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Регистрирует доступ к данным. СУБД выбирается настройкой <see cref="DatabaseOptions.Provider"/>,
    /// код запросов от неё не зависит.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, DatabaseOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new InvalidOperationException(
                "Не задана строка подключения: Database:ConnectionString в appsettings.json.");
        }

        services.AddSingleton(options);
        services.AddDbContextFactory<AppDbContext>(builder => Configure(builder, options.Provider, options.ConnectionString));
        services.AddSingleton<IAppDbContextFactory, AppDbContextFactory>();
        return services;
    }

    /// <summary>Настройка провайдера. Используется и приложением, и инструментами миграций, и тестами.</summary>
    public static DbContextOptionsBuilder Configure(DbContextOptionsBuilder builder, DatabaseProvider provider, string connectionString)
    {
        var migrations = DatabaseOptions.MigrationsAssembly(provider);
        return provider switch
        {
            DatabaseProvider.SqlServer => builder.UseSqlServer(connectionString, x => x.MigrationsAssembly(migrations)),
            DatabaseProvider.PostgreSql => builder.UseNpgsql(connectionString, x => x.MigrationsAssembly(migrations)),
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "Неизвестная СУБД")
        };
    }

    private sealed class AppDbContextFactory : IAppDbContextFactory
    {
        private readonly IDbContextFactory<AppDbContext> _inner;

        public AppDbContextFactory(IDbContextFactory<AppDbContext> inner)
        {
            _inner = inner;
        }

        public async Task<IAppDbContext> CreateAsync(CancellationToken cancellationToken = default) =>
            await _inner.CreateDbContextAsync(cancellationToken);
    }
}
