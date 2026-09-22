namespace Todo.Contracts;

public sealed record CreateTodoRequest(string Title);

public sealed record TodoResponse(
    Guid Id,
    string Title,
    bool IsDone,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);
