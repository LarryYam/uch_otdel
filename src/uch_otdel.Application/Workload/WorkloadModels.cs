using uch_otdel.Domain.Workload;

namespace uch_otdel.Application.Workload;

/// <summary>
/// Строка грида поручений. Изменяемый класс, потому что грид редактирует значения прямо в ячейках;
/// изменения применяются через <see cref="WorkloadService.UpdateAssignmentAsync"/>, где их проверяют правила Domain.
/// </summary>
public sealed class AssignmentRow
{
    public int Id { get; set; }

    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = "";
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = "";

    public string Discipline { get; set; } = "";

    public int? GroupId { get; set; }
    public string GroupName { get; set; } = "";

    public int Students { get; set; }
    public int Semester { get; set; } = 1;

    public decimal Lectures { get; set; }
    public decimal Practice { get; set; }
    public decimal Labs { get; set; }

    public bool HasExam { get; set; }
    public decimal ExamHours { get; set; }

    public decimal Total => Lectures + Practice + Labs + ExamHours;

    public AssignmentRow Clone() => (AssignmentRow)MemberwiseClone();

    /// <summary>Переносит значения из свежей строки (после сохранения) в эту, не меняя ссылку для грида.</summary>
    public void CopyFrom(AssignmentRow other)
    {
        Id = other.Id;
        TeacherId = other.TeacherId;
        TeacherName = other.TeacherName;
        DepartmentId = other.DepartmentId;
        DepartmentName = other.DepartmentName;
        Discipline = other.Discipline;
        GroupId = other.GroupId;
        GroupName = other.GroupName;
        Students = other.Students;
        Semester = other.Semester;
        Lectures = other.Lectures;
        Practice = other.Practice;
        Labs = other.Labs;
        HasExam = other.HasExam;
        ExamHours = other.ExamHours;
    }
}

/// <summary>Фильтр поручений с панели над гридом.</summary>
public sealed record AssignmentFilter(
    int? DepartmentId = null,
    int? Semester = null,
    int? TeacherId = null,
    string? Search = null);

/// <summary>Загрузка преподавателя: норма по ставке против фактических часов.</summary>
public sealed record TeacherLoad(
    int TeacherId,
    string FullName,
    string Position,
    int DepartmentId,
    string DepartmentName,
    decimal Rate,
    decimal NormHours,
    decimal ActualHours,
    decimal Percent,
    LoadStatus Status);

public sealed record DepartmentOption(int Id, string Name);

public sealed record TeacherOption(int Id, string FullName, string Position, int DepartmentId, string DepartmentName);

public sealed record GroupOption(int Id, string Name, int Course, int Students, string Specialty);

/// <summary>Одна строка для сводной таблицы: часы одного вида нагрузки по поручению.</summary>
public sealed record LoadEntry(
    string Department,
    string Teacher,
    string Discipline,
    string Group,
    string Semester,
    string LoadType,
    decimal Hours);
