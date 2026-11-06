namespace uch_otdel.Domain.Organization;

/// <summary>Кафедра.</summary>
public sealed class Department
{
    public const int NameMaxLength = 200;

    private Department()
    {
    }

    public Department(string name)
    {
        Rename(name);
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = "";

    public void Rename(string name)
    {
        name = (name ?? "").Trim();
        if (name.Length == 0)
        {
            throw new DomainException("Название кафедры не может быть пустым.");
        }
        if (name.Length > NameMaxLength)
        {
            throw new DomainException($"Название кафедры длиннее {NameMaxLength} символов.");
        }
        Name = name;
    }
}
