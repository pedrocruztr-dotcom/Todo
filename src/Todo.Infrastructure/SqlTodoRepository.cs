using Dapper;
using Todo.Application;
using Todo.Domain;

namespace Todo.Infrastructure;

public sealed class SqlTodoRepository(TodoDatabase database) : ITodoRepository
{
    public async Task<IReadOnlyList<TodoItem>> ListAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT Id, Title, CreatedAt, CompletedAt
            FROM dbo.TodoItems
            ORDER BY CASE WHEN CompletedAt IS NULL THEN 0 ELSE 1 END, CreatedAt DESC, Id DESC
            """;

        using var connection = await database.OpenAsync(cancellationToken);
        var rows = await connection.QueryAsync<TodoItem>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return rows.ToArray();
    }

    public async Task AddAsync(TodoItem item, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.TodoItems (Id, Title, CreatedAt, CompletedAt)
            VALUES (@Id, @Title, @CreatedAt, @CompletedAt)
            """;

        using var connection = await database.OpenAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, item, cancellationToken: cancellationToken));
    }

    // Um único UPDATE. Ler e depois escrever seria uma corrida que a base de dados acaba por perder.
    public async Task<TodoItem?> ToggleAsync(TodoId id, DateTimeOffset completedAt, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE dbo.TodoItems
            SET CompletedAt = CASE WHEN CompletedAt IS NULL THEN @CompletedAt ELSE NULL END
            OUTPUT INSERTED.Id, INSERTED.Title, INSERTED.CreatedAt, INSERTED.CompletedAt
            WHERE Id = @Id
            """;

        using var connection = await database.OpenAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<TodoItem>(
            new CommandDefinition(sql, new { Id = id, CompletedAt = completedAt }, cancellationToken: cancellationToken));
    }

    public async Task<bool> RemoveAsync(TodoId id, CancellationToken cancellationToken)
    {
        const string sql = "DELETE FROM dbo.TodoItems WHERE Id = @Id";

        using var connection = await database.OpenAsync(cancellationToken);
        var affected = await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));

        return affected == 1;
    }
}
