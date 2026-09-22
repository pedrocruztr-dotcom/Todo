using Todo.Domain;

namespace Todo.Application;

// A porta. Quem a implementa vive em Todo.Infrastructure e só essa camada sabe de SQL.
public interface ITodoRepository
{
    Task<IReadOnlyList<TodoItem>> ListAsync(CancellationToken cancellationToken);

    Task AddAsync(TodoItem item, CancellationToken cancellationToken);

    Task<TodoItem?> ToggleAsync(TodoId id, DateTimeOffset completedAt, CancellationToken cancellationToken);

    Task<bool> RemoveAsync(TodoId id, CancellationToken cancellationToken);
}
