namespace uch_otdel.Domain.Plans;

/// <summary>
/// Связь многие-ко-многим «учебный план ↔ группа»: часть студентов группы учится по плану.
/// </summary>
public sealed class PlanGroupLink
{
    private PlanGroupLink()
    {
    }

    public PlanGroupLink(int studyPlanId, int studentGroupId, int students)
    {
        StudyPlanId = studyPlanId;
        StudentGroupId = studentGroupId;
        SetStudents(students);
    }

    public int StudyPlanId { get; private set; }
    public int StudentGroupId { get; private set; }
    public int Students { get; private set; }

    public void SetStudents(int students)
    {
        if (students < 0)
        {
            throw new DomainException("Число студентов не может быть отрицательным.");
        }
        Students = students;
    }

    /// <summary>
    /// Сколько студентов отнести к плану при создании связи: все ещё не распределённые, но не меньше одного.
    /// </summary>
    public static int DefaultStudents(int groupStudents, int alreadyAllocated) =>
        Math.Max(1, groupStudents - alreadyAllocated);
}
