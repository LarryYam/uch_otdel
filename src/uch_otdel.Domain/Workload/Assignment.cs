using uch_otdel.Domain.Organization;

namespace uch_otdel.Domain.Workload;

/// <summary>
/// Поручение: дисциплина, закреплённая за преподавателем для группы, с часами по видам занятий.
/// Часы экзамена вычисляются по нормам и хранятся, чтобы смена норм не меняла задним числом
/// уже утверждённую нагрузку.
/// </summary>
public sealed class Assignment
{
    public const int DisciplineMaxLength = 300;
    public const decimal MaxHours = 9999m;

    private Assignment()
    {
    }

    public Assignment(int teacherId, string discipline, int semester)
    {
        TeacherId = teacherId;
        Rename(discipline);
        MoveToSemester(semester);
    }

    public int Id { get; private set; }

    public int TeacherId { get; private set; }
    public Teacher? Teacher { get; private set; }

    /// <summary>Пока строка; позже — ссылка на справочник дисциплин.</summary>
    public string Discipline { get; private set; } = "";

    public int? GroupId { get; private set; }
    public StudentGroup? Group { get; private set; }

    /// <summary>Число студентов в поручении. По умолчанию берётся из группы, но может быть меньше (подгруппа).</summary>
    public int Students { get; private set; }

    public int Semester { get; private set; } = 1;

    public decimal Lectures { get; private set; }
    public decimal Practice { get; private set; }
    public decimal Labs { get; private set; }

    public bool HasExam { get; private set; }

    /// <summary>Экзамен и консультация. Только вычисляется, вручную не вводится.</summary>
    public decimal ExamHours { get; private set; }

    public decimal Total => Lectures + Practice + Labs + ExamHours;

    public void Rename(string discipline)
    {
        discipline = (discipline ?? "").Trim();
        if (discipline.Length == 0)
        {
            throw new DomainException("Название дисциплины не может быть пустым.");
        }
        if (discipline.Length > DisciplineMaxLength)
        {
            throw new DomainException($"Название дисциплины длиннее {DisciplineMaxLength} символов.");
        }
        Discipline = discipline;
    }

    public void AssignTo(int teacherId) => TeacherId = teacherId;

    public void MoveToSemester(int semester)
    {
        if (semester is not (1 or 2))
        {
            throw new DomainException("Семестр должен быть 1 или 2.");
        }
        Semester = semester;
    }

    public void SetHours(decimal lectures, decimal practice, decimal labs)
    {
        Lectures = CheckHours(lectures, "лекций");
        Practice = CheckHours(practice, "практических занятий");
        Labs = CheckHours(labs, "лабораторных занятий");
    }

    public void ClearHours(WorkloadNorms norms)
    {
        Lectures = 0;
        Practice = 0;
        Labs = 0;
        HasExam = false;
        Recalculate(norms);
    }

    public void SetExam(bool hasExam, WorkloadNorms norms)
    {
        HasExam = hasExam;
        Recalculate(norms);
    }

    public void SetStudents(int students, WorkloadNorms norms)
    {
        if (students < 0)
        {
            throw new DomainException("Число студентов не может быть отрицательным.");
        }
        Students = students;
        Recalculate(norms);
    }

    /// <summary>Выбор группы подставляет её численность.</summary>
    public void SetGroup(StudentGroup group, WorkloadNorms norms)
    {
        ArgumentNullException.ThrowIfNull(group);
        GroupId = group.Id;
        Group = group;
        Students = group.Students;
        Recalculate(norms);
    }

    public void ClearGroup(WorkloadNorms norms)
    {
        GroupId = null;
        Group = null;
        Students = 0;
        Recalculate(norms);
    }

    public void Recalculate(WorkloadNorms norms) =>
        ExamHours = WorkloadCalculator.ExamHours(norms, HasExam, Students);

    /// <summary>Копия для другой группы: те же часы и экзамен, группа не выбрана.</summary>
    public Assignment Duplicate(WorkloadNorms norms)
    {
        var copy = new Assignment(TeacherId, Discipline, Semester)
        {
            Lectures = Lectures,
            Practice = Practice,
            Labs = Labs,
            HasExam = HasExam
        };
        copy.Recalculate(norms);
        return copy;
    }

    private static decimal CheckHours(decimal hours, string what)
    {
        if (hours < 0)
        {
            throw new DomainException($"Часы {what} не могут быть отрицательными.");
        }
        if (hours > MaxHours)
        {
            throw new DomainException($"Часы {what} больше {MaxHours}.");
        }
        return hours;
    }
}
