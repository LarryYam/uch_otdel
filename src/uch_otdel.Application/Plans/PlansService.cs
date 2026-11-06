using Microsoft.EntityFrameworkCore;
using uch_otdel.Application.Abstractions;
using uch_otdel.Application.Common;
using uch_otdel.Domain.Plans;

namespace uch_otdel.Application.Plans;

public sealed record StudyPlanRow(
    int Id, string Code, string Name, int AdmissionYear, string StudyForm,
    int GroupCount, int StudentCount);

public sealed record GroupAllocationRow(
    int Id, string Name, int Course, int Students,
    int PlanCount, int Allocated)
{
    /// <summary>Сколько студентов группы не отнесены ни к одному плану (отрицательное — распределено лишнее).</summary>
    public int Unallocated => Students - Allocated;
}

public sealed record PlanGroupLinkRow(
    int StudyPlanId, string PlanCode, string PlanName, int AdmissionYear,
    int StudentGroupId, string GroupName, int GroupStudents,
    int Students);

/// <summary>Связи «учебный план ↔ группа» (многие-ко-многим с числом студентов).</summary>
public sealed class PlansService
{
    private readonly IAppDbContextFactory _dbFactory;

    public PlansService(IAppDbContextFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<StudyPlanRow>> GetPlansAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateAsync(ct);
        return await db.StudyPlans.AsNoTracking()
            .OrderBy(p => p.Code).ThenByDescending(p => p.AdmissionYear).ThenBy(p => p.Name)
            .Select(p => new StudyPlanRow(
                p.Id, p.Code, p.Name, p.AdmissionYear, p.StudyForm,
                db.PlanGroupLinks.Count(l => l.StudyPlanId == p.Id),
                db.PlanGroupLinks.Where(l => l.StudyPlanId == p.Id).Sum(l => l.Students)))
            .ToListAsync(ct);
    }

    public async Task<List<GroupAllocationRow>> GetGroupAllocationsAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateAsync(ct);
        return await db.StudentGroups.AsNoTracking()
            .OrderBy(g => g.Course).ThenBy(g => g.Name)
            .Select(g => new GroupAllocationRow(
                g.Id, g.Name, g.Course, g.Students,
                db.PlanGroupLinks.Count(l => l.StudentGroupId == g.Id),
                db.PlanGroupLinks.Where(l => l.StudentGroupId == g.Id).Sum(l => l.Students)))
            .ToListAsync(ct);
    }

    public async Task<List<PlanGroupLinkRow>> GetLinksAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateAsync(ct);
        return await (
                from l in db.PlanGroupLinks.AsNoTracking()
                join p in db.StudyPlans on l.StudyPlanId equals p.Id
                join g in db.StudentGroups on l.StudentGroupId equals g.Id
                orderby p.Code, g.Name
                select new PlanGroupLinkRow(
                    p.Id, p.Code, p.Name, p.AdmissionYear,
                    g.Id, g.Name, g.Students,
                    l.Students))
            .ToListAsync(ct);
    }

    /// <summary>
    /// Создаёт связь. Если число студентов не указано — относит к плану всех ещё не распределённых (минимум одного).
    /// Повторная связь не создаётся: возвращается существующая и <c>Created = false</c>.
    /// </summary>
    public async Task<(PlanGroupLinkRow Link, bool Created)> ConnectAsync(
        int studyPlanId, int studentGroupId, int? students = null, CancellationToken ct = default)
    {
        var created = false;
        await using (var db = await _dbFactory.CreateAsync(ct))
        {
            var exists = await db.PlanGroupLinks.AnyAsync(
                l => l.StudyPlanId == studyPlanId && l.StudentGroupId == studentGroupId, ct);

            if (!exists)
            {
                if (!await db.StudyPlans.AnyAsync(p => p.Id == studyPlanId, ct))
                {
                    throw NotFoundException.For("Учебный план", studyPlanId);
                }

                var group = await db.StudentGroups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == studentGroupId, ct)
                            ?? throw NotFoundException.For("Группа", studentGroupId);

                var allocated = await db.PlanGroupLinks
                    .Where(l => l.StudentGroupId == studentGroupId)
                    .SumAsync(l => l.Students, ct);

                db.PlanGroupLinks.Add(new PlanGroupLink(
                    studyPlanId, studentGroupId,
                    students ?? PlanGroupLink.DefaultStudents(group.Students, allocated)));
                await db.SaveChangesAsync(ct);
                created = true;
            }
        }

        return (await GetLinkAsync(studyPlanId, studentGroupId, ct), created);
    }

    public async Task SetLinkStudentsAsync(int studyPlanId, int studentGroupId, int students, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateAsync(ct);
        var link = await db.PlanGroupLinks.FirstOrDefaultAsync(
                       l => l.StudyPlanId == studyPlanId && l.StudentGroupId == studentGroupId, ct)
                   ?? throw NotFoundException.For("Связь плана с группой", $"{studyPlanId}/{studentGroupId}");
        link.SetStudents(students);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> DisconnectAsync(int studyPlanId, int studentGroupId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateAsync(ct);
        var link = await db.PlanGroupLinks.FirstOrDefaultAsync(
            l => l.StudyPlanId == studyPlanId && l.StudentGroupId == studentGroupId, ct);
        if (link is null)
        {
            return false;
        }
        db.PlanGroupLinks.Remove(link);
        await db.SaveChangesAsync(ct);
        return true;
    }

    private async Task<PlanGroupLinkRow> GetLinkAsync(int studyPlanId, int studentGroupId, CancellationToken ct)
    {
        var links = await GetLinksAsync(ct);
        return links.FirstOrDefault(l => l.StudyPlanId == studyPlanId && l.StudentGroupId == studentGroupId)
               ?? throw NotFoundException.For("Связь плана с группой", $"{studyPlanId}/{studentGroupId}");
    }
}
