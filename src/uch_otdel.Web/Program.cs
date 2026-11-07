using Radzen;
using uch_otdel.Application;
using uch_otdel.Domain.Workload;
using uch_otdel.Infrastructure;
using uch_otdel.Infrastructure.Persistence;
using uch_otdel.Web.Components;
using uch_otdel.Web.Localization;
using uch_otdel.Web.Shell;

var builder = WebApplication.CreateBuilder(args);

// ---- Настройки ----
var norms = builder.Configuration.GetSection("Norms").Get<WorkloadNorms>() ?? new WorkloadNorms();
var database = builder.Configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new DatabaseOptions();

// ---- Слои ----
builder.Services.AddApplication(norms);
builder.Services.AddInfrastructure(database);

// ---- Интерфейс ----
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Русские надписи Radzen регистрируются до AddRadzenComponents()
builder.Services.AddSingleton<ILocalizer, RuLocalizer>();
builder.Services.AddRadzenComponents();

// Каркас: состояние своё у каждой вкладки (год, команды ленты, сообщения нижней панели)
builder.Services.AddScoped<ShellState>();

var app = builder.Build();

// Миграции и (в режиме разработки) демо-данные
await app.Services.InitializeDatabaseAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Запятая в дробных числах, русские даты
app.UseRequestLocalization("ru-RU");

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
