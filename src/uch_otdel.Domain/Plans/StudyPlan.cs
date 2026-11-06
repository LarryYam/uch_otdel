namespace uch_otdel.Domain.Plans;

/// <summary>Учебный план (образовательная программа определённого года набора).</summary>
public sealed class StudyPlan
{
    public const int CodeMaxLength = 20;
    public const int NameMaxLength = 300;
    public const int FormMaxLength = 100;

    private StudyPlan()
    {
    }

    public StudyPlan(string code, string name, int admissionYear, string studyForm)
    {
        Change(code, name, admissionYear, studyForm);
    }

    public int Id { get; private set; }
    public string Code { get; private set; } = "";
    public string Name { get; private set; } = "";
    public int AdmissionYear { get; private set; }
    public string StudyForm { get; private set; } = "";

    public void Change(string code, string name, int admissionYear, string studyForm)
    {
        code = (code ?? "").Trim();
        name = (name ?? "").Trim();
        studyForm = (studyForm ?? "").Trim();

        if (code.Length == 0 || code.Length > CodeMaxLength)
        {
            throw new DomainException($"Код плана должен быть от 1 до {CodeMaxLength} символов.");
        }
        if (name.Length == 0 || name.Length > NameMaxLength)
        {
            throw new DomainException($"Название плана должно быть от 1 до {NameMaxLength} символов.");
        }
        if (admissionYear is < 1990 or > 2100)
        {
            throw new DomainException("Год набора указан неверно.");
        }
        if (studyForm.Length > FormMaxLength)
        {
            throw new DomainException($"Форма обучения длиннее {FormMaxLength} символов.");
        }

        Code = code;
        Name = name;
        AdmissionYear = admissionYear;
        StudyForm = studyForm;
    }
}
