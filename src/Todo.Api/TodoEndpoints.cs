using Todo.Application;
using Todo.Contracts;
using Todo.Domain;

namespace Todo.Api;

public static class TodoEndpoints
{
    public static RouteGroupBuilder MapTodoEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/todos").WithTags("Todos");

        group.MapGet("/", async (TodoService todos, CancellationToken cancellationToken) =>
        {
            var items = await todos.ListAsync(cancellationToken);
            return TypedResults.Ok(items.Select(ToResponse).ToArray());
        });

        group.MapPost("/", async (CreateTodoRequest request, TodoService todos, CancellationToken cancellationToken) =>
        {
            // Parse na fronteira. Daqui para dentro o título já é válido.
            if (!TodoTitle.TryParse(request.Title, out var title))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(request.Title)] = [$"Title must not be blank and must be at most {TodoTitle.MaxLength} characters."]
                });
            }

            var created = await todos.AddAsync(title, cancellationToken);
            return Results.Created($"/todos/{created.Id}", ToResponse(created));
        });

        group.MapPost("/{id:guid}/toggle", async (Guid id, TodoService todos, CancellationToken cancellationToken) =>
        {
            var toggled = await todos.ToggleAsync(new TodoId(id), cancellationToken);
            return toggled is null ? Results.NotFound() : Results.Ok(ToResponse(toggled));
        });

        group.MapDelete("/{id:guid}", async (Guid id, TodoService todos, CancellationToken cancellationToken) =>
        {
            var removed = await todos.RemoveAsync(new TodoId(id), cancellationToken);
            return removed ? Results.NoContent() : Results.NotFound();
        });

        return group;
    }

    private static TodoResponse ToResponse(TodoItem item) =>
        new(item.Id.Value, item.Title.Value, item.IsDone, item.CreatedAt, item.CompletedAt);
}
