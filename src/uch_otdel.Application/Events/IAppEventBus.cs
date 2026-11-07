namespace uch_otdel.Application.Events;

/// <summary>
/// Рассылка событий всем открытым вкладкам приложения.
/// Сервер приложения у организации один, поэтому рассылка идёт внутри процесса.
/// </summary>
public interface IAppEventBus
{
    /// <summary>Разослать событие всем подписчикам.</summary>
    void Publish(AppEvent appEvent);

    /// <summary>Подписаться на события. Подписка снимается при Dispose результата.</summary>
    IDisposable Subscribe(Action<AppEvent> handler);
}
