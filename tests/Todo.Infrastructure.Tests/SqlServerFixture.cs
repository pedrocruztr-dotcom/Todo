using Dapper;
using Xunit;

namespace Todo.Infrastructure.Tests;

public sealed class SqlServerFixture : IAsyncLifetime
{
    private const string ConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=Todo_Tests;Integrated Security=true;TrustServerCertificate=true;Application Name=Todo.Infrastructure.Tests";

    public TodoDatabase Database { get; } = new(ConnectionString);

    public async Task InitializeAsync()
    {
        DapperTypeHandlers.Register();
        await Database.EnsureCreatedAsync(CancellationToken.None);
    }

    public async Task ClearAsync()
    {
        using var connection = await Database.OpenAsync(CancellationToken.None);
        await connection.ExecuteAsync("DELETE FROM dbo.TodoItems");
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
