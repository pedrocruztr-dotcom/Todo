using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Todo.Infrastructure;

public sealed class TodoDatabase(string connectionString)
{
    private const string SchemaScript = """
        IF OBJECT_ID(N'dbo.TodoItems', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.TodoItems
            (
                Id          uniqueidentifier  NOT NULL CONSTRAINT PK_TodoItems PRIMARY KEY,
                Title       nvarchar(200)     NOT NULL CONSTRAINT CK_TodoItems_TitleNotBlank CHECK (LEN(TRIM(Title)) > 0),
                CreatedAt   datetimeoffset(7) NOT NULL,
                CompletedAt datetimeoffset(7) NULL
            );

            CREATE INDEX IX_TodoItems_CompletedAt_CreatedAt
                ON dbo.TodoItems (CompletedAt, CreatedAt DESC);
        END
        """;

    public async Task<IDbConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    // Cria a base e a tabela ao arrancar. Serve para correr local; em produção isto são migrações.
    public async Task EnsureCreatedAsync(CancellationToken cancellationToken)
    {
        await CreateDatabaseAsync(cancellationToken);

        using var connection = await OpenAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(SchemaScript, cancellationToken: cancellationToken));
    }

    private async Task CreateDatabaseAsync(CancellationToken cancellationToken)
    {
        var databaseName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
        var master = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" };

        using var connection = new SqlConnection(master.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition(
            $"IF DB_ID(@name) IS NULL EXEC('CREATE DATABASE {Quote(databaseName)}')",
            new { name = databaseName },
            cancellationToken: cancellationToken));
    }

    private static string Quote(string identifier) => $"[{identifier.Replace("]", "]]", StringComparison.Ordinal)}]";
}
