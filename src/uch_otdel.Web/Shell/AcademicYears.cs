namespace uch_otdel.Web.Shell;

/// <summary>
/// ЗАГЛУШКА каркаса: список учебных лет вокруг текущей даты.
/// Заменится учебными годами из базы (requirements.md, Г1).
/// </summary>
public static class AcademicYears
{
    /// <summary>Учебный год, в который попадает дата (считаем, что год начинается 1 сентября).</summary>
    public static string Current(DateTime date) => Name(date.Month >= 9 ? date.Year : date.Year - 1);

    /// <summary>Два прошлых, текущий и следующий учебный год.</summary>
    public static IReadOnlyList<string> Around(DateTime date)
    {
        var start = date.Month >= 9 ? date.Year : date.Year - 1;
        return Enumerable.Range(start - 2, 4).Select(Name).ToArray();
    }

    private static string Name(int startYear) => $"{startYear}/{startYear + 1}";
}
