using System.Net;
using System.Net.Http.Json;
using Todo.Contracts;

namespace Todo.Web;

public sealed class TodoApiClient(HttpClient http)
{
    public async Task<IReadOnlyList<TodoResponse>> ListAsync(CancellationToken cancellationToken) =>
        await http.GetFromJsonAsync<TodoResponse[]>("todos", cancellationToken) ?? [];

    public async Task<TodoResponse?> AddAsync(string title, CancellationToken cancellationToken)
    {
        var response = await http.PostAsJsonAsync("todos", new CreateTodoRequest(title), cancellationToken);

        if (response.StatusCode is HttpStatusCode.BadRequest)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TodoResponse>(cancellationToken);
    }

    public async Task<bool> ToggleAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await http.PostAsync($"todos/{id}/toggle", content: null, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await http.DeleteAsync($"todos/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
