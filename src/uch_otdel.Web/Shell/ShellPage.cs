using Microsoft.AspNetCore.Components;
using uch_otdel.Application.Events;

namespace uch_otdel.Web.Shell;

/// <summary>
/// Базовый класс страниц: сообщает каркасу название экрана и команды ленты.
/// Страница: <c>@inherits ShellPage</c>, переопределить <see cref="ScreenTitle"/> и при необходимости <see cref="BuildCommands"/>.
/// </summary>
public abstract class ShellPage : ComponentBase
{
    private IReadOnlyList<ShellCommand>? _commands;

    [Inject]
    protected ShellState Shell { get; set; } = default!;

    /// <summary>Название экрана в шапке.</summary>
    protected abstract string ScreenTitle { get; }

    /// <summary>Команды ленты. Вызывается один раз за жизнь страницы.</summary>
    protected virtual IReadOnlyList<ShellCommand> BuildCommands() => [];

    protected override void OnParametersSet()
    {
        _commands ??= BuildCommands();
        Shell.SetPage(ScreenTitle, _commands);
        base.OnParametersSet();
    }

    /// <summary>Команды-заглушки: при нажатии пишут сообщение в нижнюю панель.</summary>
    protected IReadOnlyList<ShellCommand> Stubs(params (string Text, string Icon)[] commands) =>
        commands.Select(c => Stub(c.Text, c.Icon)).ToArray();

    protected ShellCommand Stub(string text, string icon) =>
        new(text, icon, () =>
        {
            Shell.Post(new AppEvent($"Команда «{text}»", "Заглушка каркаса: команда ещё не реализована.", Important: true));
            return Task.CompletedTask;
        });
}
