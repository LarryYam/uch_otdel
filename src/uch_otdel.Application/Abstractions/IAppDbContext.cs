using Microsoft.EntityFrameworkCore;
using uch_otdel.Domain.Organization;
using uch_otdel.Domain.Plans;
using uch_otdel.Domain.Workload;

namespace uch_otdel.Application.Abstractions;

/// <summary>
/// Доступ к данным для сценариев. Реализуется в Infrastructure (EF Core),
/// СУБД здесь не видна: один и тот же код работает с SQL Server и PostgreSQL.
/// </summary>
public interface IAppDbContext : IDisposable, IAsyncDisposable
{
    DbSet<Department> Departments { get; }
    DbSet<Teacher> Teachers { get; }
    DbSet<StudentGroup> StudentGroups { get; }
    DbSet<Assignment> Assignments { get; }
    DbSet<StudyPlan> StudyPlans { get; }
    DbSet<PlanGroupLink> PlanGroupLinks { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Создаёт короткоживущий контекст на одну операцию.
/// В Blazor Server нельзя держать один DbContext на всю вкладку: он копит объекты
/// и падает при одновременных действиях пользователя.
/// </summary>
public interface IAppDbContextFactory
{
    Task<IAppDbContext> CreateAsync(CancellationToken cancellationToken = default);
}
