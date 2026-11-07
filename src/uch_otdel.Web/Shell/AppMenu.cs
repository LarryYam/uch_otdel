namespace uch_otdel.Web.Shell;

/// <summary>Пункт меню.</summary>
public sealed record MenuEntry(string Text, string Icon, string Path);

/// <summary>Раздел левого меню.</summary>
public sealed record MenuSection(string Title, IReadOnlyList<MenuEntry> Entries);

/// <summary>
/// Все пункты меню в одном месте. Позже меню будет фильтроваться по правам пользователя:
/// пункты без прав не показываются (решение по теме «Каркас»).
/// Состав пунктов — предварительный.
/// </summary>
public static class AppMenu
{
    /// <summary>Левое меню: разделы по годовому циклу.</summary>
    public static IReadOnlyList<MenuSection> Sections { get; } =
    [
        new("Планы и группы",
        [
            new("Подразделения", "account_tree", "departments"),
            new("Учебные планы", "description", "plans"),
            new("Группы", "groups", "groups"),
            new("Справочники", "menu_book", "dictionaries"),
        ]),
        new("Правила расчёта",
        [
            new("Нормы", "rule", "norms"),
            new("Настройки расчёта", "tune", "calc-settings"),
            new("Настройки года", "event", "year-settings"),
        ]),
        new("Нагрузка",
        [
            new("Нагрузка", "table_chart", "workload"),
            new("Подгруппы и потоки", "call_split", "workload/streams"),
            new("Договоры ППС", "badge", "contracts"),
            new("Распределение", "assignment_ind", "workload/distribution"),
            new("Загрузка преподавателей", "bar_chart", "workload/teachers"),
        ]),
    ];

    /// <summary>Системное меню ⋮ справа в шапке.</summary>
    public static IReadOnlyList<MenuEntry> SystemMenu { get; } =
    [
        new("Справка", "help_outline", "help"),
        new("Настройки", "settings", "settings"),
        new("Журнал изменений", "history", "journal"),
        new("Администрирование", "admin_panel_settings", "admin"),
    ];

    /// <summary>Меню аккаунта (без «Выхода» — он обрабатывается отдельно).</summary>
    public static IReadOnlyList<MenuEntry> AccountMenu { get; } =
    [
        new("Профиль", "person", "profile"),
        new("Сменить пароль", "key", "profile/password"),
    ];
}
