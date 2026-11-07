using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace uch_otdel.Application.Events;

/// <summary>
/// Шина событий внутри процесса. Подписчики — вкладки Blazor (по одной подписке на вкладку).
/// Ошибка одного подписчика не мешает доставке остальным.
/// </summary>
public sealed class InProcessAppEventBus : IAppEventBus
{
    private readonly object _sync = new();
    private readonly ILogger<InProcessAppEventBus> _logger;
    private Action<AppEvent>[] _handlers = [];

    public InProcessAppEventBus(ILogger<InProcessAppEventBus>? logger = null)
    {
        _logger = logger ?? NullLogger<InProcessAppEventBus>.Instance;
    }

    public void Publish(AppEvent appEvent)
    {
        ArgumentNullException.ThrowIfNull(appEvent);

        foreach (var handler in Volatile.Read(ref _handlers))
        {
            try
            {
                handler(appEvent);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Подписчик не обработал событие «{Title}»", appEvent.Title);
            }
        }
    }

    public IDisposable Subscribe(Action<AppEvent> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (_sync)
        {
            _handlers = [.. _handlers, handler];
        }
        return new Subscription(this, handler);
    }

    private void Unsubscribe(Action<AppEvent> handler)
    {
        lock (_sync)
        {
            _handlers = _handlers.Where(h => h != handler).ToArray();
        }
    }

    private sealed class Subscription(InProcessAppEventBus bus, Action<AppEvent> handler) : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                bus.Unsubscribe(handler);
            }
        }
    }
}
