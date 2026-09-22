using Xunit;

namespace Todo.Domain.Tests;

public class TodoTitleTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void TryParse_WhenBlank_ReturnsFalse(string? candidate)
    {
        var parsed = TodoTitle.TryParse(candidate, out var title);

        Assert.False(parsed);
        Assert.Equal(default, title);
    }

    [Fact]
    public void TryParse_WhenLongerThanMaxLength_ReturnsFalse()
    {
        var parsed = TodoTitle.TryParse(new string('a', TodoTitle.MaxLength + 1), out _);

        Assert.False(parsed);
    }

    [Fact]
    public void TryParse_AtMaxLength_ReturnsTrue()
    {
        var parsed = TodoTitle.TryParse(new string('a', TodoTitle.MaxLength), out var title);

        Assert.True(parsed);
        Assert.Equal(TodoTitle.MaxLength, title.Value.Length);
    }

    [Fact]
    public void TryParse_TrimsSurroundingWhitespace()
    {
        var parsed = TodoTitle.TryParse("  buy milk  ", out var title);

        Assert.True(parsed);
        Assert.Equal("buy milk", title.Value);
    }

    [Fact]
    public void TryParse_WhenPaddingPushesPastMaxLength_MeasuresTheTrimmedValue()
    {
        var parsed = TodoTitle.TryParse($"  {new string('a', TodoTitle.MaxLength)}  ", out var title);

        Assert.True(parsed);
        Assert.Equal(TodoTitle.MaxLength, title.Value.Length);
    }
}
