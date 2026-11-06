# Учебный отдел

Веб-приложение для расчёта и распределения учебной нагрузки вуза или колледжа.

.NET 10 · Blazor Server · Radzen.Blazor · EF Core · SQL Server / PostgreSQL

## Быстрый старт

Нужны .NET 10 SDK (ставится с Visual Studio 2026) и SQL Server LocalDB
(компонент Visual Studio «SQL Server Express LocalDB») или любой SQL Server / PostgreSQL.

1. Восстановить инструменты:
   ```
   dotnet tool restore
   ```
2. Если в `src/uch_otdel.Migrations.SqlServer` ещё нет папки `Migrations`, создать первую миграцию
   для обеих СУБД (см. `CLAUDE.md`, раздел «Миграции»).
3. При необходимости поменять подключение в `src/uch_otdel.Web/appsettings.json`, секция `Database`:
   ```json
   "Database": {
     "Provider": "SqlServer",
     "ConnectionString": "Server=(localdb)\\MSSQLLocalDB;Database=uch_otdel;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```
   Для PostgreSQL: `"Provider": "PostgreSql"` и строка вида
   `Host=localhost;Database=uch_otdel;Username=postgres;Password=...`.
4. Запустить:
   ```
   dotnet run --project src/uch_otdel.Web
   ```
   При первом запуске база создаётся автоматически, в режиме разработки заполняется демо-данными.

## Документация

- [`CLAUDE.md`](CLAUDE.md) — структура, команды, правила разработки
- [`docs/domain.md`](docs/domain.md) — предметная область и термины
- [`docs/architecture.md`](docs/architecture.md) — архитектура
- [`docs/decisions.md`](docs/decisions.md) — журнал решений
