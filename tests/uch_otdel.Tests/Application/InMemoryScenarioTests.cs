namespace uch_otdel.Tests.Application;

/// <summary>Сценарии на базе в памяти: быстрые, выполняются всегда.</summary>
public class InMemoryScenarioTests
{
    [Fact]
    public async Task Teacher_loads_and_statuses() => await Scenarios.TeacherLoadsAndStatuses(await TestDatabase.InMemoryAsync());

    [Fact]
    public async Task Filters_and_case_insensitive_search() => await Scenarios.FiltersAndCaseInsensitiveSearch(await TestDatabase.InMemoryAsync());

    [Fact]
    public async Task Update_recalculates_and_validates() => await Scenarios.UpdateRecalculatesAndValidates(await TestDatabase.InMemoryAsync());

    [Fact]
    public async Task Bulk_operations() => await Scenarios.BulkOperations(await TestDatabase.InMemoryAsync());

    [Fact]
    public async Task Pivot_entries() => await Scenarios.PivotEntries(await TestDatabase.InMemoryAsync());

    [Fact]
    public async Task Plan_links() => await Scenarios.PlanLinks(await TestDatabase.InMemoryAsync());
}
