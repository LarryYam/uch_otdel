using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uch_otdel.Domain.Organization;
using uch_otdel.Domain.Plans;

namespace uch_otdel.Infrastructure.Persistence.Configurations;

internal sealed class StudyPlanConfiguration : IEntityTypeConfiguration<StudyPlan>
{
    public void Configure(EntityTypeBuilder<StudyPlan> b)
    {
        b.ToTable("StudyPlans");
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(StudyPlan.CodeMaxLength).IsRequired();
        b.Property(x => x.Name).HasMaxLength(StudyPlan.NameMaxLength).IsRequired();
        b.Property(x => x.StudyForm).HasMaxLength(StudyPlan.FormMaxLength).IsRequired();
        b.HasIndex(x => new { x.Code, x.AdmissionYear, x.Name }).IsUnique();
    }
}

internal sealed class PlanGroupLinkConfiguration : IEntityTypeConfiguration<PlanGroupLink>
{
    public void Configure(EntityTypeBuilder<PlanGroupLink> b)
    {
        b.ToTable("PlanGroupLinks");
        b.HasKey(x => new { x.StudyPlanId, x.StudentGroupId });

        b.HasOne<StudyPlan>()
            .WithMany()
            .HasForeignKey(x => x.StudyPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne<StudentGroup>()
            .WithMany()
            .HasForeignKey(x => x.StudentGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.StudentGroupId);
    }
}
