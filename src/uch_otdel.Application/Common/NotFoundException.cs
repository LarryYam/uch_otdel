namespace uch_otdel.Application.Common;

/// <summary>Запись не найдена (например, её удалил другой пользователь).</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public static NotFoundException For(string what, object id) =>
        new($"{what} (код {id}) не найден(а). Возможно, запись удалил другой пользователь — обновите страницу.");
}
