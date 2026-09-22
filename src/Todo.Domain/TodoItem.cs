namespace Todo.Domain;

// Sem flag booleana. Se tem CompletedAt está feito, e assim não existe "feito sem data".
public sealed record TodoItem(TodoId Id, TodoTitle Title, DateTimeOffset CreatedAt, DateTimeOffset? CompletedAt)
{
    public bool IsDone => CompletedAt.HasValue;
}
