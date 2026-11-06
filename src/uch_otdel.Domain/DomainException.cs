namespace uch_otdel.Domain;

/// <summary>
/// Нарушение правила предметной области. Текст сообщения показывается пользователю,
/// поэтому пишется по-русски и понятно методисту.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
