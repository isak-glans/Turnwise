using Turnwise.Domain.Entities;
using Xunit;

namespace Turnwise.Domain.Tests;

public class ConditionTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_RejectsEmptyOrWhitespaceName(string? name)
    {
        Assert.Throws<ArgumentException>(() => new Condition(name!));
    }
}
