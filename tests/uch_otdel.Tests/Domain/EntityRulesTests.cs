using uch_otdel.Domain;
using uch_otdel.Domain.Organization;
using uch_otdel.Domain.Plans;
using uch_otdel.Domain.Workload;

namespace uch_otdel.Tests.Domain;

public class EntityRulesTests
{
    private readonly WorkloadNorms _norms = new();

    [Fact]
    public void Group_selection_sets_students_and_recalculates_exam()
    {
        var group = new StudentGroup("ИС-21", 2, 25, "ИС");
        var a = new Assignment(teacherId: 1, "Базы данных", semester: 1);
        a.SetExam(true, _norms);
        Assert.Equal(0m, a.ExamHours);

        a.SetGroup(group, _norms);

        Assert.Equal(25, a.Students);
        Assert.Equal(10.3m, a.ExamHours);
    }

    [Fact]
    public void Total_includes_exam()
    {
        var a = new Assignment(1, "Базы данных", 1);
        a.SetHours(30, 15, 30);
        a.SetStudents(25, _norms);
        a.SetExam(true, _norms);

        Assert.Equal(85.3m, a.Total);
    }

    [Fact]
    public void Negative_hours_are_rejected()
    {
        var a = new Assignment(1, "Базы данных", 1);
        var ex = Assert.Throws<DomainException>(() => a.SetHours(-1, 0, 0));
        Assert.Contains("отрицательными", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public void Semester_must_be_1_or_2(int semester)
    {
        Assert.Throws<DomainException>(() => new Assignment(1, "Базы данных", semester));
    }

    [Fact]
    public void Empty_discipline_is_rejected()
    {
        Assert.Throws<DomainException>(() => new Assignment(1, "   ", 1));
    }

    [Fact]
    public void Duplicate_copies_hours_but_not_group()
    {
        var group = new StudentGroup("ИС-21", 2, 25, "ИС");
        var a = new Assignment(1, "Базы данных", 2);
        a.SetGroup(group, _norms);
        a.SetHours(30, 0, 30);
        a.SetExam(true, _norms);

        var copy = a.Duplicate(_norms);

        Assert.Equal((30m, 0m, 30m), (copy.Lectures, copy.Practice, copy.Labs));
        Assert.True(copy.HasExam);
        Assert.Null(copy.GroupId);
        Assert.Equal(0, copy.Students);
        Assert.Equal(0m, copy.ExamHours);          // нет студентов — нет часов экзамена
        Assert.Equal(2, copy.Semester);
    }

    [Fact]
    public void ClearHours_resets_hours_and_exam()
    {
        var a = new Assignment(1, "Базы данных", 1);
        a.SetHours(30, 15, 30);
        a.SetStudents(20, _norms);
        a.SetExam(true, _norms);

        a.ClearHours(_norms);

        Assert.Equal(0m, a.Total);
        Assert.False(a.HasExam);
    }

    [Theory]
    [InlineData(-0.25)]
    [InlineData(2.25)]
    public void Teacher_rate_out_of_range_is_rejected(decimal rate)
    {
        Assert.Throws<DomainException>(() => new Teacher("Иванова О.В.", "Доцент", 1, rate));
    }

    [Theory]
    [InlineData(20, 16, 4)]   // забираем нераспределённых
    [InlineData(20, 20, 1)]   // все распределены — минимум один
    [InlineData(20, 25, 1)]   // распределено лишнее — тоже один
    public void Plan_link_default_students(int groupStudents, int allocated, int expected)
    {
        Assert.Equal(expected, PlanGroupLink.DefaultStudents(groupStudents, allocated));
    }
}
