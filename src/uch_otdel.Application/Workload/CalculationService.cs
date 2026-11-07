using uch_otdel.Application.Events;

namespace uch_otdel.Application.Workload;

/// <summary>
/// Запуск расчёта нагрузки в фоне. ЗАГЛУШКА каркаса: данные не меняет, только показывает,
/// как пользователи узнают о начале и окончании расчёта через события.
/// Настоящий расчёт и запрет сохранения нагрузки на время расчёта появятся в разделе «Нагрузка».
/// </summary>
public sealed class CalculationService
{
    private readonly IAppEventBus _events;
    private int _running;

    public CalculationService(IAppEventBus events)
    {
        _events = events;
    }

    /// <summary>Сколько длится демонстрационный расчёт.</summary>
    public TimeSpan DemoDuration { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>Идёт ли расчёт сейчас.</summary>
    public bool IsRunning => Volatile.Read(ref _running) == 1;

    /// <summary>
    /// Запускает расчёт в фоне. Возвращает false, если расчёт уже идёт.
    /// </summary>
    public bool TryStart(string startedBy)
    {
        if (Interlocked.Exchange(ref _running, 1) == 1)
        {
            return false;
        }

        _events.Publish(new AppEvent(
            "Начат расчёт нагрузки",
            $"Запустил: {startedBy}. До окончания расчёта сохранение изменений нагрузки недоступно.",
            AppEventSeverity.Warning,
            Important: true,
            Author: startedBy));

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(DemoDuration);
                _events.Publish(new AppEvent(
                    "Расчёт нагрузки завершён",
                    "Демонстрация каркаса: данные не менялись.",
                    AppEventSeverity.Info,
                    Important: true,
                    Author: startedBy));
            }
            finally
            {
                Volatile.Write(ref _running, 0);
            }
        });

        return true;
    }
}
