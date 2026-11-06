namespace uch_otdel.Domain.Workload;

/// <summary>
/// Нормы расчёта нагрузки. Сейчас задаются в appsettings.json (секция "Norms"),
/// позже будут храниться в базе отдельно для каждой организации и учебного года.
/// </summary>
public sealed class WorkloadNorms
{
    /// <summary>Годовая норма часов на 1,0 ставки.</summary>
    public decimal HoursPerRate { get; set; } = 600m;

    /// <summary>Консультация перед экзаменом, часов на группу.</summary>
    public decimal ConsultationHoursPerGroup { get; set; } = 2m;

    /// <summary>Приём экзамена, часов на одного студента.</summary>
    public decimal ExamHoursPerStudent { get; set; } = 0.33m;

    /// <summary>Загрузка ниже этого процента от нормы считается недогрузом.</summary>
    public decimal UnderloadThresholdPercent { get; set; } = 90m;
}
