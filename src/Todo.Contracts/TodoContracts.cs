namespace Todo.Contracts;

// Formas que atravessam HTTP. Separadas do domínio porque têm de serializar para JSON.
public sealed record CreateTodoRequest(string Title);

public sealed record TodoResponse(
    Guid Id,
    string Title,
    bool IsDone,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);
