namespace Todo.Domain;

// Guid v7 e não o normal: vem ordenado no tempo, por isso o índice da tabela não fragmenta.
public readonly record struct TodoId(Guid Value)
{
    public static TodoId New() => new(Guid.CreateVersion7());

    public override string ToString() => Value.ToString();
}
