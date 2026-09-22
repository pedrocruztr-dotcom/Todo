using Xunit;

namespace Todo.Domain.Tests;

public class TodoItemTests
{
    [Fact]
    public void IsDone_WhenCompletedAtIsNull_IsFalse()
    {
        var item = Pending();

        Assert.False(item.IsDone);
    }

    [Fact]
    public void IsDone_WhenCompletedAtIsSet_IsTrue()
    {
        var item = Pending() with { CompletedAt = DateTimeOffset.UnixEpoch };

        Assert.True(item.IsDone);
    }

    private static TodoItem Pending()
    {
        Assert.True(TodoTitle.TryParse("buy milk", out var title));
        return new TodoItem(TodoId.New(), title, DateTimeOffset.UnixEpoch, CompletedAt: null);
    }
}
