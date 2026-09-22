namespace Todo.Domain;

public readonly record struct TodoId(Guid Value)
{
    public static TodoId New() => new(Guid.CreateVersion7());

    public override string ToString() => Value.ToString();
}
