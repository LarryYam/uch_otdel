namespace uch_otdel.Application.Events;

/// <summary>Важность события; от неё зависит значок в нижней панели.</summary>
public enum AppEventSeverity
{
    Info,
    Warning,
    Error
}

/// <summary>
/// Событие, которое видят пользователи в нижней панели: «начат расчёт», «строку изменил Петров» и т. п.
/// </summary>
/// <param name="Title">Короткий заголовок.</param>
/// <param name="Text">Подробности (необязательно).</param>
/// <param name="Severity">Важность.</param>
/// <param name="Important">Принудительно открыть нижнюю панель у получателя.</param>
/// <param name="Author">Кто вызвал событие (имя пользователя), если известно.</param>
public sealed record AppEvent(
    string Title,
    string? Text = null,
    AppEventSeverity Severity = AppEventSeverity.Info,
    bool Important = false,
    string? Author = null)
{
    /// <summary>Время события, UTC.</summary>
    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;
}
