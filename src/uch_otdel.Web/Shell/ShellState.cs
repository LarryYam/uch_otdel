using uch_otdel.Application.Events;

namespace uch_otdel.Web.Shell;

/// <summary>Команда ленты в шапке. Набор команд объявляет каждая страница.</summary>
public sealed record ShellCommand(string Text, string Icon, Func<Task> Execute);

/// <summary>
/// Состояние каркаса одной вкладки браузера (scoped = одна вкладка Blazor Server):
/// выбранный учебный год, название и команды текущего экрана, сообщения нижней панели.
/// Год у каждой вкладки свой, поэтому в двух вкладках можно держать разные годы.
/// </summary>
public sealed class ShellState : IDisposable
{
    private const int MaxMessages = 200;

    private readonly object _sync = new();
    private readonly List<AppEvent> _messages = new();
    private readonly IDisposable _subscription;

    public ShellState(IAppEventBus events)
    {
        Years = AcademicYears.Around(DateTime.UtcNow);
        Year = AcademicYears.Current(DateTime.UtcNow);
        _subscription = events.Subscribe(Post);
    }

    /// <summary>Вызывается при любом изменении; каркас перерисовывается.</summary>
    public event Action? Changed;

    /// <summary>Пользователь. ЗАГЛУШКА до появления входа в систему.</summary>
    public string UserName => "Методист (вход не настроен)";

    /// <summary>Учебные годы для выбора. ЗАГЛУШКА до появления учебных лет в базе.</summary>
    public IReadOnlyList<string> Years { get; }

    public string Year { get; private set; }

    public string Title { get; private set; } = "";

    public IReadOnlyList<ShellCommand> Commands { get; private set; } = [];

    public bool IsPanelOpen { get; private set; }

    /// <summary>Сообщения, пришедшие, пока панель была закрыта.</summary>
    public int UnreadCount { get; private set; }

    /// <summary>Сообщения нижней панели, новые сверху.</summary>
    public IReadOnlyList<AppEvent> Messages
    {
        get
        {
            lock (_sync)
            {
                return _messages.ToArray();
            }
        }
    }

    public void SetPage(string title, IReadOnlyList<ShellCommand> commands)
    {
        if (Title == title && ReferenceEquals(Commands, commands))
        {
            return;
        }
        Title = title;
        Commands = commands;
        Changed?.Invoke();
    }

    public void SetYear(string year)
    {
        if (Year == year || !Years.Contains(year))
        {
            return;
        }
        Year = year;
        Changed?.Invoke();
    }

    /// <summary>
    /// Добавить сообщение в нижнюю панель. Важное сообщение открывает панель принудительно.
    /// Вызывается и из других потоков (рассылка событий).
    /// </summary>
    public void Post(AppEvent message)
    {
        lock (_sync)
        {
            _messages.Insert(0, message);
            if (_messages.Count > MaxMessages)
            {
                _messages.RemoveAt(_messages.Count - 1);
            }
            if (message.Important)
            {
                IsPanelOpen = true;
            }
            if (!IsPanelOpen)
            {
                UnreadCount++;
            }
        }
        Changed?.Invoke();
    }

    public void TogglePanel() => SetPanelOpen(!IsPanelOpen);

    public void SetPanelOpen(bool open)
    {
        lock (_sync)
        {
            IsPanelOpen = open;
            if (open)
            {
                UnreadCount = 0;
            }
        }
        Changed?.Invoke();
    }

    public void ClearMessages()
    {
        lock (_sync)
        {
            _messages.Clear();
            UnreadCount = 0;
        }
        Changed?.Invoke();
    }

    public void Dispose() => _subscription.Dispose();
}
