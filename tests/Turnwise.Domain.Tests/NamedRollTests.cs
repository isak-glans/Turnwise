using Turnwise.Domain.Entities;
using Turnwise.Domain.ValueObjects;
using Xunit;

namespace Turnwise.Domain.Tests;

public class NamedRollTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_RejectsEmptyOrWhitespaceName(string? name)
    {
        var formula = DiceFormula.Parse("1d6");

        Assert.Throws<ArgumentException>(() => new NamedRoll(name!, formula));
    }

    [Fact]
    public void Name_LongerThanMaxIsSilentlyTruncated()
    {
        var roll = new NamedRoll("Longsword", DiceFormula.Parse("1d8+3"));

        roll.Name = new string('x', NamedRoll.MaxNameLength + 50);

        Assert.Equal(NamedRoll.MaxNameLength, roll.Name.Length);
    }

    [Fact]
    public void Constructor_NameLongerThanMaxIsSilentlyTruncated()
    {
        var roll = new NamedRoll(new string('x', NamedRoll.MaxNameLength + 50), DiceFormula.Parse("1d8+3"));

        Assert.Equal(NamedRoll.MaxNameLength, roll.Name.Length);
    }
}
