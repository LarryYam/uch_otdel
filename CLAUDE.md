# Учебный отдел — расчёт и распределение учебной нагрузки

Веб-приложение для вузов и колледжей: распределение поручений (дисциплина × группа × преподаватель),
расчёт часов по нормам, контроль загрузки преподавателей, связи учебных планов с группами.
Пользователи: учебный отдел, заведующие кафедрами, преподаватели. Поставка: в первой версии — установка у вуза
(одна организация — одна база), облако — позже. Функции и приоритеты: `docs/requirements.md`.

Предметная область и термины: `docs/domain.md`. Архитектура: `docs/architecture.md`.
Принятые решения и их причины: `docs/decisions.md` — сверяйся с ним перед тем, как предлагать другой подход.

## Стек

.NET 10, Blazor Server (интерактивность на всех страницах), Radzen.Blazor (только бесплатные компоненты, MIT),
EF Core 10, SQL Server и PostgreSQL, xUnit.

## Структура решения

```
src/uch_otdel.Domain                  сущности и правила расчёта; без зависимостей
src/uch_otdel.Application             сценарии (WorkloadService, PlansService), IAppDbContext, DTO для экранов
src/uch_otdel.Infrastructure          EF Core: AppDbContext, Configurations, выбор СУБД, демо-данные
src/uch_otdel.Migrations.SqlServer    миграции для SQL Server (генерируются, руками не правятся)
src/uch_otdel.Migrations.PostgreSql   миграции для PostgreSQL
src/uch_otdel.Web                     Blazor + Radzen, Program.cs (сборка всех слоёв)
tests/uch_otdel.Tests                 тесты Domain и сценариев; интеграционные — на обеих СУБД
```

Зависимости только внутрь: Web → Application → Domain; Infrastructure → Application;
Migrations.* → Infrastructure. Domain не ссылается ни на что, включая EF Core.

## Команды

```
dotnet build
dotnet test
dotnet run --project src/uch_otdel.Web
dotnet list package --vulnerable --include-transitive
```

### Миграции

После любого изменения сущностей или Configurations — новая миграция **для обеих СУБД** с одним именем:

```
dotnet tool restore
dotnet ef migrations add <Имя> --project src/uch_otdel.Migrations.SqlServer --startup-project src/uch_otdel.Web -- --provider SqlServer
dotnet ef migrations add <Имя> --project src/uch_otdel.Migrations.PostgreSql --startup-project src/uch_otdel.Web -- --provider PostgreSql
```

СУБД для миграций выбирает `Infrastructure/Persistence/DesignTimeDbContextFactory.cs` по аргументу `--provider`.
Приложение применяет миграции при запуске (`Database:MigrateOnStartup`).

### Интеграционные тесты на настоящих СУБД

`DatabaseProviderTests` берёт строки подключения из переменных `UCH_TEST_SQLSERVER` и `UCH_TEST_POSTGRES`;
без них тест для этой СУБД пропускается. Тест удаляет и пересоздаёт базу — только отдельная тестовая база.
В CI (`.github/workflows/ci.yml`) обе СУБД поднимаются в контейнерах.

## Правила архитектуры

- Правила предметной области (формулы, ограничения, статусы) — только в Domain. Нарушение правила — `DomainException`
  с понятным пользователю русским сообщением.
- Сущности меняются методами (`SetHours`, `AssignTo`, …), сеттеры приватные. Значения, зависящие от норм
  (часы экзамена), пересчитываются внутри этих методов.
- Сценарий = публичный метод сервиса в Application: загрузить → вызвать правила Domain → сохранить.
  Проверка прав пользователя будет здесь же.
- Каждый метод сервиса открывает свой контекст через `IAppDbContextFactory` и закрывает его
  (`await using`). Никогда не держать DbContext в поле компонента или сервиса: Blazor Server.
- Экраны получают DTO из Application (`AssignmentRow`, `TeacherLoad`, …), а не сущности.
- Хранимые процедуры, триггеры и SQL, специфичный для одной СУБД, не используем. Всё через LINQ.
- Поиск без учёта регистра: `.ToLower().Contains(text.ToLowerInvariant())` — работает на обеих СУБД.
- Для decimal всегда явная точность (по умолчанию 9,2 задано в `AppDbContext.ConfigureConventions`).
- Дата и время — в UTC.
- Будущий ИИ-помощник вызывает те же сценарии Application, что и интерфейс, а не базу напрямую.

## Интерфейс

- Страницы тонкие: показывают данные и вызывают сценарии. Расчётов в `.razor` нет.
- Только бесплатные компоненты Radzen; платные сборки и сторонние платные гриды не подключать.
- Русские надписи Radzen — `Web/Localization/RuLocalizer.cs`. Культура приложения ru-RU.
- Большие таблицы (десятки тысяч строк и больше) — серверный режим грида: `LoadData` + запрос к базе,
  а не загрузка всего в память.
- Ошибки `DomainException` и `NotFoundException` показывать уведомлением, а не падением страницы.

## Тесты

- Каждое новое правило Domain — юнит-тест.
- Новый сценарий — проверка в `tests/.../Application/Scenarios.cs`: она автоматически выполняется
  и в памяти, и на SQL Server, и на PostgreSQL.
- Демо-данные (`DemoDataSeeder`) — основа тестов: общий итог 2 992,4 ч, Ахметов и Петров перегружены,
  Ким недогружен, в ИС-32 4 нераспределённых студента. Меняешь демо-данные — обнови тесты.

## Стиль

- Имена в коде — английские; комментарии, сообщения пользователю, интерфейс — русские.
- Пространства имён по папкам: `uch_otdel.Domain.Workload`, `uch_otdel.Application.Plans` и т.д.
- Nullable включён, предупреждений компилятора быть не должно.

## Как работать

- Для заметных изменений (новая сущность, изменение схемы, новый экран) сначала кратко опиши план.
- Изменения — в отдельной ветке и через Pull Request; в `main` напрямую не коммитить.
- Перед завершением: `dotnet build` и `dotnet test` без ошибок и предупреждений; если менялась модель — миграции для обеих СУБД.
- В отчёте честно разделяй, что проверено (сборка, тесты, запуск), а что нет.
- Не добавлять в репозиторий строки подключения с паролями, ключи API и реальные персональные данные.
