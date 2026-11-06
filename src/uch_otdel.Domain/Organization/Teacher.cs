namespace uch_otdel.Domain.Organization;

/// <summary>Преподаватель и его ставка на кафедре.</summary>
public sealed class Teacher
{
    public const int FullNameMaxLength = 150;
    public const int PositionMaxLength = 100;
    public const decimal MaxRate = 2m;

    private Teacher()
    {
    }

    public Teacher(string fullName, string position, int departmentId, decimal rate)
    {
        Rename(fullName);
        ChangePosition(position);
        DepartmentId = departmentId;
        ChangeRate(rate);
    }

    public int Id { get; private set; }
    public string FullName { get; private set; } = "";
    public string Position { get; private set; } = "";

    public int DepartmentId { get; private set; }
    public Department? Department { get; private set; }

    /// <summary>Доля ставки: 1,0, 0,5, 0,25 и т.д.</summary>
    public decimal Rate { get; private set; }

    public void Rename(string fullName)
    {
        fullName = (fullName ?? "").Trim();
        if (fullName.Length == 0)
        {
            throw new DomainException("ФИО преподавателя не может быть пустым.");
        }
        if (fullName.Length > FullNameMaxLength)
        {
            throw new DomainException($"ФИО длиннее {FullNameMaxLength} символов.");
        }
        FullName = fullName;
    }

    public void ChangePosition(string position)
    {
        position = (position ?? "").Trim();
        if (position.Length > PositionMaxLength)
        {
            throw new DomainException($"Должность длиннее {PositionMaxLength} символов.");
        }
        Position = position;
    }

    public void ChangeRate(decimal rate)
    {
        if (rate < 0 || rate > MaxRate)
        {
            throw new DomainException($"Ставка должна быть от 0 до {MaxRate}.");
        }
        Rate = rate;
    }

    public void MoveToDepartment(int departmentId) => DepartmentId = departmentId;
}
