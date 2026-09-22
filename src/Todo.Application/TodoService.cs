using Todo.Domain;

namespace Todo.Application;

// Decide o quando. O relógio entra por aqui para os testes poderem fixar a hora.
public sealed class TodoService(ITodoRepository repository, TimeProvider clock)
{
    public Task<IReadOnlyList<TodoItem>> ListAsync(CancellationToken cancellationToken) =>
        repository.ListAsync(cancellationToken);

    public async Task<TodoItem> AddAsync(TodoTitle title, CancellationToken cancellationToken)
    {
        var item = new TodoItem(TodoId.New(), title, clock.GetUtcNow(), CompletedAt: null);
        await repository.AddAsync(item, cancellationToken);
        return item;
    }

    public Task<TodoItem?> ToggleAsync(TodoId id, CancellationToken cancellationToken) =>
        repository.ToggleAsync(id, clock.GetUtcNow(), cancellationToken);

    public Task<bool> RemoveAsync(TodoId id, CancellationToken cancellationToken) =>
        repository.RemoveAsync(id, cancellationToken);
}
