using System.Net;
using System.Net.Http.Json;
using Todo.Contracts;
using Xunit;

namespace Todo.Api.Tests;

public sealed class TodoEndpointsTests(TodoApiFactory factory) : IClassFixture<TodoApiFactory>, IAsyncLifetime
{
    private readonly HttpClient client = factory.CreateClient();

    public async Task InitializeAsync()
    {
        var existing = await client.GetFromJsonAsync<TodoResponse[]>("/todos") ?? [];

        foreach (var item in existing)
        {
            await client.DeleteAsync($"/todos/{item.Id}");
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Post_WhenTitleIsValid_CreatesAndReturnsTheItem()
    {
        var response = await client.PostAsJsonAsync("/todos", new CreateTodoRequest("buy milk"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.Equal("buy milk", created?.Title);
        Assert.False(created?.IsDone);
        Assert.Equal($"/todos/{created?.Id}", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Post_WhenTitleIsBlank_ReturnsValidationProblem(string title)
    {
        var response = await client.PostAsJsonAsync("/todos", new CreateTodoRequest(title));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>();
        Assert.True(problem?.Errors.ContainsKey(nameof(CreateTodoRequest.Title)));
    }

    [Fact]
    public async Task Post_WhenTitleIsTooLong_ReturnsValidationProblem()
    {
        var response = await client.PostAsJsonAsync("/todos", new CreateTodoRequest(new string('a', 201)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_TrimsTheTitleBeforeStoringIt()
    {
        var response = await client.PostAsJsonAsync("/todos", new CreateTodoRequest("  buy milk  "));

        var created = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.Equal("buy milk", created?.Title);
    }

    [Fact]
    public async Task Get_ReturnsItemsThatWerePosted()
    {
        await client.PostAsJsonAsync("/todos", new CreateTodoRequest("buy milk"));

        var listed = await client.GetFromJsonAsync<TodoResponse[]>("/todos");

        Assert.Equal("buy milk", Assert.Single(listed!).Title);
    }

    [Fact]
    public async Task Toggle_WhenPending_MarksItDone()
    {
        var created = await CreateAsync("buy milk");

        var response = await client.PostAsync($"/todos/{created.Id}/toggle", content: null);

        var toggled = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.True(toggled?.IsDone);
        Assert.NotNull(toggled?.CompletedAt);
    }

    [Fact]
    public async Task Toggle_WhenAlreadyDone_MarksItPending()
    {
        var created = await CreateAsync("buy milk");
        await client.PostAsync($"/todos/{created.Id}/toggle", content: null);

        var response = await client.PostAsync($"/todos/{created.Id}/toggle", content: null);

        var toggled = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.False(toggled?.IsDone);
        Assert.Null(toggled?.CompletedAt);
    }

    [Fact]
    public async Task Toggle_WhenIdIsUnknown_ReturnsNotFound()
    {
        var response = await client.PostAsync($"/todos/{Guid.NewGuid()}/toggle", content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenItemExists_ReturnsNoContentAndRemovesIt()
    {
        var created = await CreateAsync("buy milk");

        var response = await client.DeleteAsync($"/todos/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await client.GetFromJsonAsync<TodoResponse[]>("/todos") ?? []);
    }

    [Fact]
    public async Task Delete_WhenIdIsUnknown_ReturnsNotFound()
    {
        var response = await client.DeleteAsync($"/todos/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenIdIsNotAGuid_ReturnsNotFound()
    {
        var response = await client.PostAsync("/todos/not-a-guid/toggle", content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<TodoResponse> CreateAsync(string title)
    {
        var response = await client.PostAsJsonAsync("/todos", new CreateTodoRequest(title));
        return (await response.Content.ReadFromJsonAsync<TodoResponse>())!;
    }

    private sealed record ValidationProblemResponse(Dictionary<string, string[]> Errors);
}
