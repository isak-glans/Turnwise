using Turnwise.Domain.Entities;
using Xunit;

namespace Turnwise.Domain.Tests;

public class CounterTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_RejectsEmptyOrWhitespaceName(string? name)
    {
        Assert.Throws<ArgumentException>(() => new Counter(name!, 0, 3));
    }

    [Fact]
    public void Constructor_RejectsNegativeMax()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Counter("Ki", 0, -1));
    }

    [Fact]
    public void Constructor_ClampsCurrentIntoRange()
    {
        var counter = new Counter("Ki", 999, 3);

        Assert.Equal(3, counter.Current);
    }

    [Fact]
    public void SetMax_RejectsNegativeValue()
    {
        var counter = new Counter("Ki", 1, 3);

        Assert.Throws<ArgumentOutOfRangeException>(() => counter.SetMax(-1));
    }

    [Fact]
    public void Adjust_ClampsAtZeroAndMax()
    {
        var counter = new Counter("Ki", 1, 3);

        counter.Adjust(-10);
        Assert.Equal(0, counter.Current);

        counter.Adjust(10);
        Assert.Equal(3, counter.Current);
    }

    [Fact]
    public void Name_LongerThanMaxIsSilentlyTruncated()
    {
        var counter = new Counter("Ki", 0, 3);

        counter.Name = new string('x', Counter.MaxNameLength + 50);

        Assert.Equal(Counter.MaxNameLength, counter.Name.Length);
    }

    [Fact]
    public void Constructor_NameLongerThanMaxIsSilentlyTruncated()
    {
        var counter = new Counter(new string('x', Counter.MaxNameLength + 50), 0, 3);

        Assert.Equal(Counter.MaxNameLength, counter.Name.Length);
    }
}
