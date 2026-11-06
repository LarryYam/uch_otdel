using uch_otdel.Domain.Workload;

namespace uch_otdel.Tests.Domain;

public class WorkloadCalculatorTests
{
    private readonly WorkloadNorms _norms = new();

    [Theory]
    [InlineData(25, 10.3)]   // 2 + 0,33 × 25 = 10,25 → 10,3 (округление от нуля)
    [InlineData(20, 8.6)]    // 2 + 6,6
    [InlineData(1, 2.3)]     // 2 + 0,33 = 2,33 → 2,3
    public void ExamHours_consultation_plus_students(int students, decimal expected)
    {
        Assert.Equal(expected, WorkloadCalculator.ExamHours(_norms, hasExam: true, students));
    }

    [Fact]
    public void ExamHours_zero_without_exam_or_students()
    {
        Assert.Equal(0m, WorkloadCalculator.ExamHours(_norms, hasExam: false, students: 25));
        Assert.Equal(0m, WorkloadCalculator.ExamHours(_norms, hasExam: true, students: 0));
    }

    [Fact]
    public void NormHours_by_rate()
    {
        Assert.Equal(600m, WorkloadCalculator.NormHours(_norms, 1m));
        Assert.Equal(300m, WorkloadCalculator.NormHours(_norms, 0.5m));
        Assert.Equal(450m, WorkloadCalculator.NormHours(_norms, 0.75m));
    }

    [Theory]
    [InlineData(600, 601, LoadStatus.Overload)]
    [InlineData(600, 600, LoadStatus.Normal)]
    [InlineData(600, 540, LoadStatus.Normal)]     // ровно 90 % — ещё норма
    [InlineData(600, 539.9, LoadStatus.Underload)]
    [InlineData(0, 10, LoadStatus.Overload)]      // ставки нет, а часы есть
    [InlineData(0, 0, LoadStatus.Normal)]
    public void Status_by_percent_of_norm(decimal norm, decimal actual, LoadStatus expected)
    {
        Assert.Equal(expected, WorkloadCalculator.Status(_norms, norm, actual));
    }

    [Fact]
    public void Percent_rounded_to_tenth()
    {
        Assert.Equal(112.3m, WorkloadCalculator.Percent(600m, 673.8m));
        Assert.Equal(0m, WorkloadCalculator.Percent(0m, 100m));
    }
}
