using uch_otdel.Application.Plans;
using uch_otdel.Application.Workload;
using uch_otdel.Domain;
using uch_otdel.Domain.Workload;

namespace uch_otdel.Tests.Application;

/// <summary>
/// Проверки сценариев на демо-данных. Одни и те же проверки выполняются
/// на базе в памяти (<see cref="InMemoryScenarioTests"/>) и на настоящих СУБД (<see cref="DatabaseProviderTests"/>):
/// так ловятся запросы, которые одна СУБД переводит в SQL, а другая — нет.
/// </summary>
internal static class Scenarios
{
    // Итог демо-данных, совпадает с прототипом
    public const decimal DemoTotalHours = 2992.4m;

    public static async Task TeacherLoadsAndStatuses(TestDatabase db)
    {
        var service = new WorkloadService(db, db.Norms);
        var loads = await service.GetTeacherLoadsAsync();

        Assert.Equal(6, loads.Count);
        Assert.Equal(DemoTotalHours, loads.Sum(l => l.ActualHours));

        var status = loads.ToDictionary(l => l.FullName, l => l.Status);
        Assert.Equal(LoadStatus.Overload, status["Ахметов Е.С."]);
        Assert.Equal(LoadStatus.Overload, status["Петров В.Н."]);
        Assert.Equal(LoadStatus.Underload, status["Ким А.Л."]);
        Assert.Equal(LoadStatus.Normal, status["Иванова О.В."]);
        Assert.Equal(LoadStatus.Normal, status["Сериков Д.А."]);
        Assert.Equal(LoadStatus.Normal, status["Жумабаева Г.К."]);

        var departments = await service.GetDepartmentsAsync();
        var math = departments.Single(d => d.Name == "Высшая математика");
        var mathLoads = await service.GetTeacherLoadsAsync(math.Id);
        Assert.Equal(new[] { "Жумабаева Г.К.", "Петров В.Н." }, mathLoads.Select(l => l.FullName).OrderBy(x => x));
    }

    public static async Task FiltersAndCaseInsensitiveSearch(TestDatabase db)
    {
        var service = new WorkloadService(db, db.Norms);

        var all = await service.GetAssignmentsAsync(new AssignmentFilter());
        Assert.Equal(43, all.Count);
        Assert.Equal(DemoTotalHours, all.Sum(r => r.Total));

        // Кириллица в другом регистре: должно находиться и на SQL Server, и на PostgreSQL
        var found = await service.GetAssignmentsAsync(new AssignmentFilter(Search: "БАЗЫ"));
        Assert.Equal(2, found.Count);
        Assert.All(found, r => Assert.Equal("Базы данных", r.Discipline));

        var byGroup = await service.GetAssignmentsAsync(new AssignmentFilter(Search: "мис-11"));
        Assert.Equal(3, byGroup.Count);

        var second = await service.GetAssignmentsAsync(new AssignmentFilter(Semester: 2));
        Assert.All(second, r => Assert.Equal(2, r.Semester));
        Assert.Equal(43, second.Count + (await service.GetAssignmentsAsync(new AssignmentFilter(Semester: 1))).Count);
    }

    public static async Task UpdateRecalculatesAndValidates(TestDatabase db)
    {
        var service = new WorkloadService(db, db.Norms);
        var groups = await service.GetGroupsAsync();
        var row = (await service.GetAssignmentsAsync(new AssignmentFilter(Search: "Информатика"))).First();

        // Ставим экзамен и группу ИС-21 (25 студентов) — экзамен считается сам
        var edit = row.Clone();
        edit.HasExam = true;
        edit.GroupId = groups.Single(g => g.Name == "ИС-21").Id;
        edit.Lectures = 15;
        var saved = await service.UpdateAssignmentAsync(edit);

        Assert.Equal("ИС-21", saved.GroupName);
        Assert.Equal(25, saved.Students);
        Assert.Equal(10.3m, saved.ExamHours);
        Assert.Equal(15m, saved.Lectures);

        // Подгруппа: меньше студентов — меньше часов экзамена
        var sub = saved.Clone();
        sub.Students = 12;
        saved = await service.UpdateAssignmentAsync(sub);
        Assert.Equal(6m, saved.ExamHours);   // 2 + 0,33 × 12 = 5,96 → 6,0

        // Правила Domain работают и через сценарий
        var bad = saved.Clone();
        bad.Labs = -5;
        await Assert.ThrowsAsync<DomainException>(() => service.UpdateAssignmentAsync(bad));
        Assert.Equal(saved.Labs, (await service.GetAssignmentAsync(saved.Id)).Labs);
    }

    public static async Task BulkOperations(TestDatabase db)
    {
        var service = new WorkloadService(db, db.Norms);
        var teachers = await service.GetTeachersAsync();
        var kim = teachers.Single(t => t.FullName == "Ким А.Л.");
        var akhmetov = teachers.Single(t => t.FullName == "Ахметов Е.С.");

        var akhmetovRows = await service.GetAssignmentsAsync(new AssignmentFilter(TeacherId: akhmetov.Id));
        var ids = akhmetovRows.Take(3).Select(r => r.Id).ToList();

        Assert.Equal(3, await service.ReassignAsync(ids, kim.Id));
        Assert.Equal(0, await service.ReassignAsync(ids, kim.Id));    // уже у Ким

        var kimRows = await service.GetAssignmentsAsync(new AssignmentFilter(TeacherId: kim.Id));
        Assert.Equal(8, kimRows.Count);
        Assert.All(kimRows.Where(r => ids.Contains(r.Id)), r => Assert.Equal("Информационные системы", r.DepartmentName));

        await service.SetSemesterAsync(ids, 2);
        Assert.All(await service.GetAssignmentsAsync(new AssignmentFilter(TeacherId: kim.Id, Semester: 2)),
            r => Assert.Equal(2, r.Semester));

        var copy = await service.DuplicateAsync(ids[0]);
        Assert.NotEqual(ids[0], copy.Id);
        Assert.Null(copy.GroupId);

        var cleared = await service.ClearHoursAsync(copy.Id);
        Assert.Equal(0m, cleared.Total);

        var added = await service.AddAssignmentAsync(kim.Id, 2);
        Assert.Equal("Новая дисциплина", added.Discipline);

        Assert.Equal(2, await service.DeleteAsync(new[] { copy.Id, added.Id }));
        Assert.Equal(43, (await service.GetAssignmentsAsync(new AssignmentFilter())).Count);

        // Общий итог после передачи не изменился — часы просто ушли к другому преподавателю
        Assert.Equal(DemoTotalHours, (await service.GetTeacherLoadsAsync()).Sum(l => l.ActualHours));

        await service.SetTeacherRateAsync(kim.Id, 1m);
        Assert.Equal(600m, (await service.GetTeacherLoadsAsync()).Single(l => l.TeacherId == kim.Id).NormHours);
    }

    public static async Task PivotEntries(TestDatabase db)
    {
        var service = new WorkloadService(db, db.Norms);
        var entries = await service.GetLoadEntriesAsync();
        Assert.Equal(DemoTotalHours, entries.Sum(e => e.Hours));
        Assert.Contains(entries, e => e.LoadType == "Экзамены и консультации");
    }

    public static async Task PlanLinks(TestDatabase db)
    {
        var plans = new PlansService(db);

        var allPlans = await plans.GetPlansAsync();
        var groups = await plans.GetGroupAllocationsAsync();
        var links = await plans.GetLinksAsync();
        Assert.Equal(9, allPlans.Count);
        Assert.Equal(8, groups.Count);
        Assert.Equal(14, links.Count);

        // Многие-ко-многим: у ИС-22 три плана; у плана 6B06101 (2025) две группы и 39 студентов
        Assert.Equal(3, groups.Single(g => g.Name == "ИС-22").PlanCount);
        var plan2025 = allPlans.Single(p => p.Code == "6B06101" && p.AdmissionYear == 2025);
        Assert.Equal((2, 39), (plan2025.GroupCount, plan2025.StudentCount));

        // В демо-данных не распределены 4 студента ИС-32
        Assert.Equal(new[] { ("ИС-32", 4) },
            groups.Where(g => g.Unallocated != 0).Select(g => (g.Name, g.Unallocated)));

        // Новая связь забирает нераспределённых, повтор не дублируется
        var dual = allPlans.Single(p => p.Code == "6B06102");
        var is32 = groups.Single(g => g.Name == "ИС-32");
        var (link, created) = await plans.ConnectAsync(dual.Id, is32.Id);
        Assert.True(created);
        Assert.Equal(4, link.Students);
        var (again, createdAgain) = await plans.ConnectAsync(dual.Id, is32.Id);
        Assert.False(createdAgain);
        Assert.Equal(link, again);
        Assert.Equal(0, (await plans.GetGroupAllocationsAsync()).Single(g => g.Id == is32.Id).Unallocated);

        // Связь с полностью распределённой группой — минимум один студент, видно «лишнего»
        var security = allPlans.Single(p => p.Code == "6B06104");
        var is41 = groups.Single(g => g.Name == "ИС-41");
        var (extra, _) = await plans.ConnectAsync(security.Id, is41.Id);
        Assert.Equal(1, extra.Students);
        Assert.Equal(-1, (await plans.GetGroupAllocationsAsync()).Single(g => g.Id == is41.Id).Unallocated);

        await plans.SetLinkStudentsAsync(security.Id, is41.Id, 0);
        Assert.Equal(0, (await plans.GetLinksAsync()).Single(l => l.StudyPlanId == security.Id && l.StudentGroupId == is41.Id).Students);
        await Assert.ThrowsAsync<DomainException>(() => plans.SetLinkStudentsAsync(security.Id, is41.Id, -1));

        Assert.True(await plans.DisconnectAsync(security.Id, is41.Id));
        Assert.False(await plans.DisconnectAsync(security.Id, is41.Id));
        Assert.Equal(15, (await plans.GetLinksAsync()).Count);
    }
}
