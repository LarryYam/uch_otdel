namespace uch_otdel.Domain.Workload;

public enum LoadStatus
{
    Underload,
    Normal,
    Overload
}

/// <summary>
/// Правила расчёта нагрузки. Чистые функции без доступа к данным —
/// их легко покрыть тестами и сверить с результатами старой программы.
/// </summary>
public static class WorkloadCalculator
{
    /// <summary>Часы на экзамен: консультация на группу + приём по числу студентов, с округлением до 0,1.</summary>
    public static decimal ExamHours(WorkloadNorms norms, bool hasExam, int students)
    {
        if (!hasExam || students <= 0)
        {
            return 0m;
        }

        var hours = norms.ConsultationHoursPerGroup + norms.ExamHoursPerStudent * students;
        return Math.Round(hours, 1, MidpointRounding.AwayFromZero);
    }

    /// <summary>Годовая норма часов преподавателя по его ставке.</summary>
    public static decimal NormHours(WorkloadNorms norms, decimal rate) => rate * norms.HoursPerRate;

    /// <summary>Процент выполнения нормы, с точностью до 0,1.</summary>
    public static decimal Percent(decimal normHours, decimal actualHours) =>
        normHours == 0 ? 0 : Math.Round(actualHours / normHours * 100m, 1, MidpointRounding.AwayFromZero);

    public static LoadStatus Status(WorkloadNorms norms, decimal normHours, decimal actualHours)
    {
        if (normHours <= 0)
        {
            return actualHours > 0 ? LoadStatus.Overload : LoadStatus.Normal;
        }

        var percent = actualHours / normHours * 100m;
        if (percent > 100m)
        {
            return LoadStatus.Overload;
        }

        return percent < norms.UnderloadThresholdPercent ? LoadStatus.Underload : LoadStatus.Normal;
    }
}
