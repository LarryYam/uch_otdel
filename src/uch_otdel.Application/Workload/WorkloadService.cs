using Microsoft.EntityFrameworkCore;
using uch_otdel.Application.Abstractions;
using uch_otdel.Application.Common;
using uch_otdel.Domain.Workload;

namespace uch_otdel.Application.Workload;

/// <summary>
/// Сценарии распределения нагрузки. Каждый метод открывает свой короткий контекст данных,
/// поэтому сервис безопасно использовать из Blazor Server при одновременных действиях.
/// Правила расчёта — в Domain; здесь только загрузка, вызов правил и сохранение.
/// </summary>
public sealed class WorkloadService
{
    private const string NewDisciplineName = "Новая дисциплина";

    private readonly IAppDbContextFactory _dbFactory;
    private readonly WorkloadNorms _norms;

    public WorkloadService(IAppDbContextFactory dbFactory, WorkloadNorms norms)
    {
        _dbFactory = dbFactory;
        _norms = norms;
    }

    public WorkloadNorms Norms => _norms;

    // ================= Справочники =================

    public async Task<List<DepartmentOption>> GetDepartmentsAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateAsync(ct);
        return await db.Departments.AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentOption(d.Id, d.Name))
            .ToListAsync(ct);
    }

    public async Task<List<TeacherOption>> GetTeachersAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateAsync(ct);
        return await db.Teachers.AsNoTracking()
            .OrderBy(t => t.FullName)
            .Select(t => new TeacherOption(t.Id, t.FullName, t.Position, t.DepartmentId, t.Department!.Name))
            .ToListAsync(ct);
    }

    public async Task<List<GroupOption>> GetGroupsAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateAsync(ct);
        return await db.StudentGroups.AsNoTracking()
            .OrderBy(g => g.Course).ThenBy(g => g.Name)
            .Select(g => new GroupOption(g.Id, g.Name, g.Course, g.Students, g.Specialty))
            .ToListAsync(ct);
    }

    // ================= Поручения =================

    public async Task<List<AssignmentRow>> GetAssignmentsAsync(AssignmentFilter filter, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateAsync(ct);
        IQueryable<Assignment> query = db.Assignments.AsNoTracking();

        if (filter.DepartmentId is int departmentId)
        {
            query = query.Where(a => a.Teacher!.DepartmentId == departmentId);
        }
        if (filter.Semester is int semester)
        {
            query = query.Where(a => a.Semester == semester);
        }
        if (filter.TeacherId is int teacherId)
        {
            query = query.Where(a => a.TeacherId == teacherId);
        }

        var search = filter.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            // ToLower() переводится в LOWER() на обеих СУБД: поиск без учёта регистра
            // и на SQL Server, и на PostgreSQL (где сравнение по умолчанию регистрозависимое).
            var text = search.ToLowerInvariant();
            query = query.Where(a =>
                a.Discipline.ToLower().Contains(text)
                || a.Teacher!.FullName.ToLower().Contains(text)
                || (a.Group != null && a.Group.Name.ToLower().Contains(text)));
        }

        return await ProjectRows(query)
            .OrderBy(r => r.TeacherName).ThenBy(r => r.Semester).ThenBy(r => r.Discipline).ThenBy(r => r.Id)
            .ToListAsync(ct);
    }

    public async Task<AssignmentRow> GetAssignmentAsync(int id, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateAsync(ct);
        return await ProjectRows(db.Assignments.AsNoTracking().Where(a => a.Id == id)).FirstOrDefaultAsync(ct)
               ?? throw NotFoundException.For("Поручение", id);
    }

    /// <summary>
    /// Применяет изменения строки грида. Правила Domain проверяют значения;
    /// при смене группы число студентов берётся из группы. Возвращает строку в том виде, как она сохранена.
    /// </summary>
    public async Task<AssignmentRow> UpdateAssignmentAsync(AssignmentRow row, CancellationToken ct = default)
    {
        await using (var db = await _dbFactory.CreateAsync(ct))
        {
            var a = await db.Assignments.FirstOrDefaultAsync(x => x.Id == row.Id, ct)
                    ?? throw NotFoundException.For("Поручение", row.Id);

            if (row.Discipline != a.Discipline)
            {
                a.Rename(row.Discipline);
            }

            if (row.TeacherId != a.TeacherId)
            {
                if (!await db.Teachers.AnyAsync(t => t.Id == row.TeacherId, ct))
                {
                    throw NotFoundException.For("Преподаватель", row.TeacherId);
                }
                a.AssignTo(row.TeacherId);
            }

            if (row.Semester != a.Semester)
            {
                a.MoveToSemester(row.Semester);
            }

            if (row.GroupId != a.GroupId)
            {
                if (row.GroupId is int groupId)
                {
                    var group = await db.StudentGroups.FirstOrDefaultAsync(g => g.Id == groupId, ct)
                                ?? throw NotFoundException.For("Группа", groupId);
                    a.SetGroup(group, _norms);
                }
                else
                {
                    a.ClearGroup(_norms);
                }
            }
            else if (row.Students != a.Students)
            {
                a.SetStudents(row.Students, _norms);
            }

            if (row.Lectures != a.Lectures || row.Practice != a.Practice || row.Labs != a.Labs)
            {
                a.SetHours(row.Lectures, row.Practice, row.Labs);
            }

            if (row.HasExam != a.HasExam)
            {
                a.SetExam(row.HasExam, _norms);
            }

            await db.SaveChangesAsync(ct);
        }

        return await GetAssignmentAsync(row.Id, ct);
    }

    public async Task<AssignmentRow> AddAssignmentAsync(int teacherId, int semester, CancellationToken ct = default)
    {
        int id;
        await using (var db = await _dbFactory.CreateAsync(ct))
        {
            if (!await db.Teachers.AnyAsync(t => t.Id == teacherId, ct))
            {
                throw NotFoundException.For("Преподаватель", teacherId);
            }

            var a = new Assignment(teacherId, NewDisciplineName, semester is 1 or 2 ? semester : 1);
            db.Assignments.Add(a);
            await db.SaveChangesAsync(ct);
            id = a.Id;
        }
        return await GetAssignmentAsync(id, ct);
    }

    public async Task<AssignmentRow> DuplicateAsync(int id, CancellationToken ct = default)
    {
        int copyId;
        await using (var db = await _dbFactory.CreateAsync(ct))
        {
            var source = await db.Assignments.FirstOrDefaultAsync(x => x.Id == id, ct)
                         ?? throw NotFoundException.For("Поручение", id);
            var copy = source.Duplicate(_norms);
            db.Assignments.Add(copy);
            await db.SaveChangesAsync(ct);
            copyId = copy.Id;
        }
        return await GetAssignmentAsync(copyId, ct);
    }

    public async Task<AssignmentRow> ClearHoursAsync(int id, CancellationToken ct = default)
    {
        await using (var db = await _dbFactory.CreateAsync(ct))
        {
            var a = await db.Assignments.FirstOrDefaultAsync(x => x.Id == id, ct)
                    ?? throw NotFoundException.For("Поручение", id);
            a.ClearHours(_norms);
            await db.SaveChangesAsync(ct);
        }
        return await GetAssignmentAsync(id, ct);
    }

    /// <summary>Передаёт поручения преподавателю. Возвращает число действительно переданных.</summary>
    public async Task<int> ReassignAsync(IReadOnlyCollection<int> ids, int teacherId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateAsync(ct);
        if (!await db.Teachers.AnyAsync(t => t.Id == teacherId, ct))
        {
            throw NotFoundException.For("Преподаватель", teacherId);
        }

        var items = await db.Assignments.Where(a => ids.Contains(a.Id) && a.TeacherId != teacherId).ToListAsync(ct);
        foreach (var a in items)
        {
            a.AssignTo(teacherId);
        }
        await db.SaveChangesAsync(ct);
        return items.Count;
    }

    public async Task<int> SetSemesterAsync(IReadOnlyCollection<int> ids, int semester, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateAsync(ct);
        var items = await db.Assignments.Where(a => ids.Contains(a.Id) && a.Semester != semester).ToListAsync(ct);
        foreach (var a in items)
        {
            a.MoveToSemester(semester);
        }
        await db.SaveChangesAsync(ct);
        return items.Count;
    }

    public async Task<int> DeleteAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateAsync(ct);
        var items = await db.Assignments.Where(a => ids.Contains(a.Id)).ToListAsync(ct);
        db.Assignments.RemoveRange(items);
        await db.SaveChangesAsync(ct);
        return items.Count;
    }

    // ================= Преподаватели =================

    public async Task SetTeacherRateAsync(int teacherId, decimal rate, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateAsync(ct);
        var teacher = await db.Teachers.FirstOrDefaultAsync(t => t.Id == teacherId, ct)
                      ?? throw NotFoundException.For("Преподаватель", teacherId);
        teacher.ChangeRate(rate);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Загрузка преподавателей: сумма часов считается в базе, норма и статус — правилами Domain.</summary>
    public async Task<List<TeacherLoad>> GetTeacherLoadsAsync(int? departmentId = null, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateAsync(ct);

        var teachers = db.Teachers.AsNoTracking();
        if (departmentId is int d)
        {
            teachers = teachers.Where(t => t.DepartmentId == d);
        }

        var data = await teachers
            .OrderBy(t => t.FullName)
            .Select(t => new
            {
                t.Id,
                t.FullName,
                t.Position,
                t.DepartmentId,
                DepartmentName = t.Department!.Name,
                t.Rate,
                Actual = db.Assignments
                    .Where(a => a.TeacherId == t.Id)
                    .Sum(a => a.Lectures + a.Practice + a.Labs + a.ExamHours)
            })
            .ToListAsync(ct);

        return data.Select(x =>
        {
            var norm = WorkloadCalculator.NormHours(_norms, x.Rate);
            return new TeacherLoad(
                x.Id, x.FullName, x.Position, x.DepartmentId, x.DepartmentName, x.Rate,
                norm, x.Actual,
                WorkloadCalculator.Percent(norm, x.Actual),
                WorkloadCalculator.Status(_norms, norm, x.Actual));
        }).ToList();
    }

    // ================= Сводная =================

    public async Task<List<LoadEntry>> GetLoadEntriesAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateAsync(ct);
        var rows = await ProjectRows(db.Assignments.AsNoTracking()).ToListAsync(ct);

        var result = new List<LoadEntry>(rows.Count * 3);
        foreach (var r in rows)
        {
            void Add(string type, decimal hours)
            {
                if (hours > 0)
                {
                    result.Add(new LoadEntry(r.DepartmentName, r.TeacherName, r.Discipline, r.GroupName,
                        $"{r.Semester} семестр", type, hours));
                }
            }

            Add("Лекции", r.Lectures);
            Add("Практические", r.Practice);
            Add("Лабораторные", r.Labs);
            Add("Экзамены и консультации", r.ExamHours);
        }
        return result;
    }

    private static IQueryable<AssignmentRow> ProjectRows(IQueryable<Assignment> query) =>
        query.Select(a => new AssignmentRow
        {
            Id = a.Id,
            TeacherId = a.TeacherId,
            TeacherName = a.Teacher!.FullName,
            DepartmentId = a.Teacher.DepartmentId,
            DepartmentName = a.Teacher.Department!.Name,
            Discipline = a.Discipline,
            GroupId = a.GroupId,
            GroupName = a.Group != null ? a.Group.Name : "",
            Students = a.Students,
            Semester = a.Semester,
            Lectures = a.Lectures,
            Practice = a.Practice,
            Labs = a.Labs,
            HasExam = a.HasExam,
            ExamHours = a.ExamHours
        });
}
