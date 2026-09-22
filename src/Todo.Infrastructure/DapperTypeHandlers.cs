using System.Data;
using Dapper;
using Todo.Domain;

namespace Todo.Infrastructure;

public static class DapperTypeHandlers
{
    private static bool registered;

    public static void Register()
    {
        if (registered)
        {
            return;
        }

        SqlMapper.AddTypeHandler(new TodoIdHandler());
        SqlMapper.AddTypeHandler(new TodoTitleHandler());
        registered = true;
    }

    private sealed class TodoIdHandler : SqlMapper.TypeHandler<TodoId>
    {
        public override void SetValue(IDbDataParameter parameter, TodoId value)
        {
            parameter.DbType = DbType.Guid;
            parameter.Value = value.Value;
        }

        public override TodoId Parse(object value) => new((Guid)value);
    }

    private sealed class TodoTitleHandler : SqlMapper.TypeHandler<TodoTitle>
    {
        public override void SetValue(IDbDataParameter parameter, TodoTitle value)
        {
            parameter.DbType = DbType.String;
            // Tamanho explícito. Sem isto o parâmetro vai como nvarchar(4000) e o índice deixa de ser usado.
            parameter.Size = TodoTitle.MaxLength;
            parameter.Value = value.Value;
        }

        public override TodoTitle Parse(object value)
        {
            if (!TodoTitle.TryParse((string)value, out var title))
            {
                throw new InvalidOperationException($"Stored title is not a valid {nameof(TodoTitle)}.");
            }

            return title;
        }
    }
}
