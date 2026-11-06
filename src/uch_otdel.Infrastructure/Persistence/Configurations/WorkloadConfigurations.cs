using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uch_otdel.Domain.Workload;

namespace uch_otdel.Infrastructure.Persistence.Configurations;

internal sealed class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> b)
    {
        b.ToTable("Assignments");
        b.HasKey(x => x.Id);

        b.Property(x => x.Discipline).HasMaxLength(Assignment.DisciplineMaxLength).IsRequired();
        b.Property(x => x.Lectures).HasPrecision(7, 2);
        b.Property(x => x.Practice).HasPrecision(7, 2);
        b.Property(x => x.Labs).HasPrecision(7, 2);
        b.Property(x => x.ExamHours).HasPrecision(7, 2);

        // Итог вычисляется в коде, в базе не хранится
        b.Ignore(x => x.Total);

        b.HasOne(x => x.Teacher)
            .WithMany()
            .HasForeignKey(x => x.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Group)
            .WithMany()
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasIndex(x => new { x.TeacherId, x.Semester });
        b.HasIndex(x => x.Discipline);
    }
}
