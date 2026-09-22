using Todo.Domain;
using Xunit;

namespace Todo.Infrastructure.Tests;

public sealed class SqlTodoRepositoryTests(SqlServerFixture fixture) : IClassFixture<SqlServerFixture>, IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 9, 22, 18, 0, 0, TimeSpan.Zero);

    private readonly SqlTodoRepository repository = new(fixture.Database);

    public Task InitializeAsync() => fixture.ClearAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AddAsync_ThenListAsync_ReturnsTheStoredItem()
    {
        var item = Pending("buy milk");

        await repository.AddAsync(item, CancellationToken.None);
        var listed = await repository.ListAsync(CancellationToken.None);

        Assert.Equal(item, Assert.Single(listed));
    }

    [Fact]
    public async Task ListAsync_OrdersPendingBeforeDone()
    {
        var pending = Pending("still to do") with { CreatedAt = Now.AddHours(-1) };
        var completed = Pending("already done");
        await repository.AddAsync(pending, CancellationToken.None);
        await repository.AddAsync(completed, CancellationToken.None);
        await repository.ToggleAsync(completed.Id, Now, CancellationToken.None);

        var listed = await repository.ListAsync(CancellationToken.None);

        Assert.Equal([pending.Id, completed.Id], listed.Select(item => item.Id));
    }

    [Fact]
    public async Task ToggleAsync_WhenPending_StampsCompletedAt()
    {
        var item = Pending("buy milk");
        await repository.AddAsync(item, CancellationToken.None);

        var toggled = await repository.ToggleAsync(item.Id, Now, CancellationToken.None);

        Assert.Equal(Now, toggled?.CompletedAt);
    }

    [Fact]
    public async Task ToggleAsync_WhenAlreadyDone_ClearsCompletedAt()
    {
        var item = Pending("buy milk");
        await repository.AddAsync(item, CancellationToken.None);
        await repository.ToggleAsync(item.Id, Now, CancellationToken.None);

        var toggled = await repository.ToggleAsync(item.Id, Now, CancellationToken.None);

        Assert.Null(toggled?.CompletedAt);
    }

    [Fact]
    public async Task ToggleAsync_WhenIdIsUnknown_ReturnsNull()
    {
        var toggled = await repository.ToggleAsync(TodoId.New(), Now, CancellationToken.None);

        Assert.Null(toggled);
    }

    [Fact]
    public async Task RemoveAsync_WhenIdIsUnknown_ReturnsFalse()
    {
        var removed = await repository.RemoveAsync(TodoId.New(), CancellationToken.None);

        Assert.False(removed);
    }

    [Fact]
    public async Task RemoveAsync_WhenItemExists_DropsIt()
    {
        var item = Pending("buy milk");
        await repository.AddAsync(item, CancellationToken.None);

        var removed = await repository.RemoveAsync(item.Id, CancellationToken.None);

        Assert.True(removed);
        Assert.Empty(await repository.ListAsync(CancellationToken.None));
    }

    [Fact]
    public async Task ToggleAsync_WhenTwoCallersRace_AppliesEachExactlyOnce()
    {
        var item = Pending("buy milk");
        await repository.AddAsync(item, CancellationToken.None);

        var results = await Task.WhenAll(
            repository.ToggleAsync(item.Id, Now, CancellationToken.None),
            repository.ToggleAsync(item.Id, Now, CancellationToken.None));

        Assert.Single(results, result => result?.CompletedAt is not null);
        Assert.Single(results, result => result?.CompletedAt is null);
        Assert.Null(Assert.Single(await repository.ListAsync(CancellationToken.None)).CompletedAt);
    }

    private static TodoItem Pending(string title)
    {
        Assert.True(TodoTitle.TryParse(title, out var parsed));
        return new TodoItem(TodoId.New(), parsed, Now, CompletedAt: null);
    }
}
