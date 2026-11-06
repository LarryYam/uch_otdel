namespace uch_otdel.Domain.Organization;

/// <summary>Учебная группа (контингент).</summary>
public sealed class StudentGroup
{
    public const int NameMaxLength = 50;
    public const int SpecialtyMaxLength = 200;

    private StudentGroup()
    {
    }

    public StudentGroup(string name, int course, int students, string specialty)
    {
        Rename(name);
        ChangeCourse(course);
        ChangeStudents(students);
        ChangeSpecialty(specialty);
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = "";
    public int Course { get; private set; }
    public int Students { get; private set; }
    public string Specialty { get; private set; } = "";

    public void Rename(string name)
    {
        name = (name ?? "").Trim();
        if (name.Length == 0)
        {
            throw new DomainException("Название группы не может быть пустым.");
        }
        if (name.Length > NameMaxLength)
        {
            throw new DomainException($"Название группы длиннее {NameMaxLength} символов.");
        }
        Name = name;
    }

    public void ChangeCourse(int course)
    {
        if (course is < 1 or > 7)
        {
            throw new DomainException("Курс должен быть от 1 до 7.");
        }
        Course = course;
    }

    public void ChangeStudents(int students)
    {
        if (students < 0)
        {
            throw new DomainException("Число студентов не может быть отрицательным.");
        }
        Students = students;
    }

    public void ChangeSpecialty(string specialty)
    {
        specialty = (specialty ?? "").Trim();
        if (specialty.Length > SpecialtyMaxLength)
        {
            throw new DomainException($"Название специальности длиннее {SpecialtyMaxLength} символов.");
        }
        Specialty = specialty;
    }
}
