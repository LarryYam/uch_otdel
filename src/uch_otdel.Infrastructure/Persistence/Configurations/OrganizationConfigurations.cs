using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uch_otdel.Domain.Organization;

namespace uch_otdel.Infrastructure.Persistence.Configurations;

internal sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> b)
    {
        b.ToTable("Departments");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(Department.NameMaxLength).IsRequired();
        b.HasIndex(x => x.Name).IsUnique();
    }
}

internal sealed class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> b)
    {
        b.ToTable("Teachers");
        b.HasKey(x => x.Id);
        b.Property(x => x.FullName).HasMaxLength(Teacher.FullNameMaxLength).IsRequired();
        b.Property(x => x.Position).HasMaxLength(Teacher.PositionMaxLength).IsRequired();
        b.Property(x => x.Rate).HasPrecision(4, 2);

        b.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.FullName);
    }
}

internal sealed class StudentGroupConfiguration : IEntityTypeConfiguration<StudentGroup>
{
    public void Configure(EntityTypeBuilder<StudentGroup> b)
    {
        b.ToTable("StudentGroups");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(StudentGroup.NameMaxLength).IsRequired();
        b.Property(x => x.Specialty).HasMaxLength(StudentGroup.SpecialtyMaxLength).IsRequired();
        b.HasIndex(x => x.Name).IsUnique();
    }
}
