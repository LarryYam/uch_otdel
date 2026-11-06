using System.Globalization;
using Radzen;
using Radzen.Blazor;

namespace uch_otdel.Web.Localization;

/// <summary>
/// Русские надписи встроенных элементов Radzen: фильтры, выбор колонок, группировка, пейджер, сводная таблица.
/// Регистрируется в Program.cs до AddRadzenComponents(). Ключ, которого нет в словаре, остаётся английским.
/// </summary>
public sealed class RuLocalizer : ILocalizer
{
    private static readonly Dictionary<string, string> Ru = new()
    {
        // ---- Грид: фильтры ----
        [nameof(RadzenStrings.DataGrid_FilterText)] = "Фильтр",
        [nameof(RadzenStrings.DataGrid_EnumFilterSelectText)] = "Выберите…",
        [nameof(RadzenStrings.DataGrid_EnumNullFilterText)] = "Нет значения",
        [nameof(RadzenStrings.DataGrid_AndOperatorText)] = "И",
        [nameof(RadzenStrings.DataGrid_OrOperatorText)] = "ИЛИ",
        [nameof(RadzenStrings.DataGrid_ApplyFilterText)] = "Применить",
        [nameof(RadzenStrings.DataGrid_ClearFilterText)] = "Очистить",
        [nameof(RadzenStrings.DataGrid_EqualsText)] = "Равно",
        [nameof(RadzenStrings.DataGrid_NotEqualsText)] = "Не равно",
        [nameof(RadzenStrings.DataGrid_LessThanText)] = "Меньше",
        [nameof(RadzenStrings.DataGrid_LessThanOrEqualsText)] = "Меньше или равно",
        [nameof(RadzenStrings.DataGrid_GreaterThanText)] = "Больше",
        [nameof(RadzenStrings.DataGrid_GreaterThanOrEqualsText)] = "Больше или равно",
        [nameof(RadzenStrings.DataGrid_EndsWithText)] = "Заканчивается на",
        [nameof(RadzenStrings.DataGrid_ContainsText)] = "Содержит",
        [nameof(RadzenStrings.DataGrid_DoesNotContainText)] = "Не содержит",
        [nameof(RadzenStrings.DataGrid_InText)] = "В списке",
        [nameof(RadzenStrings.DataGrid_NotInText)] = "Не в списке",
        [nameof(RadzenStrings.DataGrid_StartsWithText)] = "Начинается с",
        [nameof(RadzenStrings.DataGrid_IsNotNullText)] = "Заполнено",
        [nameof(RadzenStrings.DataGrid_IsNullText)] = "Не заполнено",
        [nameof(RadzenStrings.DataGrid_IsEmptyText)] = "Пусто",
        [nameof(RadzenStrings.DataGrid_IsNotEmptyText)] = "Не пусто",
        [nameof(RadzenStrings.DataGrid_CustomText)] = "Своё условие",
        [nameof(RadzenStrings.DataGrid_FilterToggleAriaLabel)] = "Открыть фильтр",
        [nameof(RadzenStrings.DataGrid_LogicalOperatorAriaLabel)] = "логический оператор",
        [nameof(RadzenStrings.DataGrid_FilterOperatorAriaLabel)] = "оператор фильтра",
        [nameof(RadzenStrings.DataGrid_SecondFilterOperatorAriaLabel)] = "второй оператор фильтра",
        [nameof(RadzenStrings.DataGrid_FilterValueAriaLabel)] = "значение фильтра",
        [nameof(RadzenStrings.DataGrid_SecondFilterValueAriaLabel)] = "второе значение фильтра",

        // ---- Грид: прочее ----
        [nameof(RadzenStrings.DataGrid_EmptyText)] = "Нет записей",
        [nameof(RadzenStrings.DataGrid_ColumnsShowingText)] = "колонок показано",
        [nameof(RadzenStrings.DataGrid_AllColumnsText)] = "Все",
        [nameof(RadzenStrings.DataGrid_ColumnsText)] = "Колонки",
        [nameof(RadzenStrings.DataGrid_SelectVisibleColumnsAriaLabel)] = "выбор видимых колонок",
        [nameof(RadzenStrings.DataGrid_RemoveGroupAriaLabel)] = "Убрать группировку",
        [nameof(RadzenStrings.DataGrid_GroupPanelText)] = "Перетащите сюда заголовок колонки, чтобы сгруппировать",
        [nameof(RadzenStrings.DataGrid_ExpandAllTitle)] = "Развернуть все",
        [nameof(RadzenStrings.DataGrid_CollapseAllTitle)] = "Свернуть все",
        [nameof(RadzenStrings.DataGrid_ExpandAllGroupsTitle)] = "Развернуть все группы",
        [nameof(RadzenStrings.DataGrid_CollapseAllGroupsTitle)] = "Свернуть все группы",
        [nameof(RadzenStrings.DataGrid_ExpandChildItemAriaLabel)] = "Раскрыть строку",
        [nameof(RadzenStrings.DataGrid_ExpandGroupAriaLabel)] = "Раскрыть группу",

        // ---- Выпадающие списки ----
        [nameof(RadzenStrings.DropDown_RemoveChipTitle)] = "Убрать",
        [nameof(RadzenStrings.DropDown_ClearAriaLabel)] = "Очистить",
        [nameof(RadzenStrings.DropDown_SearchAriaLabel)] = "Поиск",
        [nameof(RadzenStrings.DropDown_EmptyAriaLabel)] = "Пусто",
        [nameof(RadzenStrings.DropDown_SelectedItemsText)] = "выбрано",
        [nameof(RadzenStrings.ListBox_EmptyText)] = "Нет записей",

        // ---- Пейджер ----
        [nameof(RadzenStrings.Pager_FirstPageTitle)] = "Первая страница",
        [nameof(RadzenStrings.Pager_FirstPageAriaLabel)] = "Перейти на первую страницу",
        [nameof(RadzenStrings.Pager_PrevPageTitle)] = "Предыдущая страница",
        [nameof(RadzenStrings.Pager_PrevPageAriaLabel)] = "Перейти на предыдущую страницу",
        [nameof(RadzenStrings.Pager_NextPageTitle)] = "Следующая страница",
        [nameof(RadzenStrings.Pager_NextPageAriaLabel)] = "Перейти на следующую страницу",
        [nameof(RadzenStrings.Pager_LastPageTitle)] = "Последняя страница",
        [nameof(RadzenStrings.Pager_LastPageAriaLabel)] = "Перейти на последнюю страницу",
        [nameof(RadzenStrings.Pager_PageTitleFormat)] = "Страница {0}",
        [nameof(RadzenStrings.Pager_PageAriaLabelFormat)] = "Перейти на страницу {0}",
        [nameof(RadzenStrings.Pager_PageSizeText)] = "строк на странице",
        [nameof(RadzenStrings.Pager_NavigationAriaLabel)] = "Страницы",
        [nameof(RadzenStrings.Pager_PagingSummaryFormat)] = "Страница {0} из {1} (записей: {2})",
        [nameof(RadzenStrings.Pager_ReloadTitle)] = "Обновить",
        [nameof(RadzenStrings.Pager_ReloadAriaLabel)] = "Обновить текущую страницу",

        // ---- Числовой редактор ----
        [nameof(RadzenStrings.Numeric_UpAriaLabel)] = "Больше",
        [nameof(RadzenStrings.Numeric_DownAriaLabel)] = "Меньше",

        // ---- Сводная таблица ----
        [nameof(RadzenStrings.PivotDataGrid_EmptyText)] = "Нет данных",
        [nameof(RadzenStrings.PivotDataGrid_OthersText)] = "Прочее",
        [nameof(RadzenStrings.PivotDataGrid_FieldsPickerHeaderText)] = "Настройка полей",
        [nameof(RadzenStrings.PivotDataGrid_RowsText)] = "Строки",
        [nameof(RadzenStrings.PivotDataGrid_ColumnsText)] = "Колонки",
        [nameof(RadzenStrings.PivotDataGrid_AggregatesText)] = "Значения",
        [nameof(RadzenStrings.PivotDataGrid_SortAriaLabelFormat)] = "Сортировать по {0}",
        [nameof(RadzenStrings.PivotDataGrid_FilterText)] = "Фильтр",
        [nameof(RadzenStrings.PivotDataGrid_EnumFilterSelectText)] = "Выберите…",
        [nameof(RadzenStrings.PivotDataGrid_ApplyText)] = "Применить",
        [nameof(RadzenStrings.PivotDataGrid_ClearText)] = "Очистить",
        [nameof(RadzenStrings.PivotDataGrid_FilterOperatorAriaLabel)] = "Оператор фильтра",
        [nameof(RadzenStrings.PivotDataGrid_FilterValueAriaLabel)] = "Значение фильтра",
        [nameof(RadzenStrings.PivotDataGrid_SecondFilterOperatorAriaLabel)] = "Второй оператор фильтра",
        [nameof(RadzenStrings.PivotDataGrid_SecondFilterValueAriaLabel)] = "Второе значение фильтра",
        [nameof(RadzenStrings.PivotDataGrid_LogicalOperatorAriaLabel)] = "Логический оператор",
        [nameof(RadzenStrings.PivotDataGrid_AndOperatorText)] = "И",
        [nameof(RadzenStrings.PivotDataGrid_OrOperatorText)] = "ИЛИ",
        [nameof(RadzenStrings.PivotDataGrid_EqualsText)] = "Равно",
        [nameof(RadzenStrings.PivotDataGrid_NotEqualsText)] = "Не равно",
        [nameof(RadzenStrings.PivotDataGrid_LessThanText)] = "Меньше",
        [nameof(RadzenStrings.PivotDataGrid_LessThanOrEqualsText)] = "Меньше или равно",
        [nameof(RadzenStrings.PivotDataGrid_GreaterThanText)] = "Больше",
        [nameof(RadzenStrings.PivotDataGrid_GreaterThanOrEqualsText)] = "Больше или равно",
        [nameof(RadzenStrings.PivotDataGrid_EndsWithText)] = "Заканчивается на",
        [nameof(RadzenStrings.PivotDataGrid_ContainsText)] = "Содержит",
        [nameof(RadzenStrings.PivotDataGrid_DoesNotContainText)] = "Не содержит",
        [nameof(RadzenStrings.PivotDataGrid_InText)] = "В списке",
        [nameof(RadzenStrings.PivotDataGrid_NotInText)] = "Не в списке",
        [nameof(RadzenStrings.PivotDataGrid_StartsWithText)] = "Начинается с",
        [nameof(RadzenStrings.PivotDataGrid_IsNotNullText)] = "Заполнено",
        [nameof(RadzenStrings.PivotDataGrid_IsNullText)] = "Не заполнено",
        [nameof(RadzenStrings.PivotDataGrid_IsEmptyText)] = "Пусто",
        [nameof(RadzenStrings.PivotDataGrid_IsNotEmptyText)] = "Не пусто",
        [nameof(RadzenStrings.PivotDataGrid_CustomText)] = "Своё условие",
    };

    public string? Get(string key, CultureInfo culture) =>
        Ru.TryGetValue(key, out var value) ? value : null;
}
