using Microsoft.EntityFrameworkCore;
using uch_otdel.Application.Abstractions;
using uch_otdel.Domain.Organization;
using uch_otdel.Domain.Plans;
using uch_otdel.Domain.Workload;

namespace uch_otdel.Infrastructure.Persistence;

/// <summary>
/// Контекст EF Core. Не знает, какая СУБД используется: провайдер выбирается
/// при регистрации (см. <see cref="DependencyInjection"/>), настройки таблиц — в Configurations.
/// </summary>
public sealed class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<StudentGroup> StudentGroups => Set<StudentGroup>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<StudyPlan> StudyPlans => Set<StudyPlan>();
    public DbSet<PlanGroupLink> PlanGroupLinks => Set<PlanGroupLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Одинаковая точность для всех decimal, если для свойства не указана своя.
        // Без этого SQL Server и PostgreSQL выбрали бы разные типы по умолчанию.
        configurationBuilder.Properties<decimal>().HavePrecision(9, 2);
    }
}
