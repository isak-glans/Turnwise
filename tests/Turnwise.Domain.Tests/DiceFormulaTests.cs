using Turnwise.Domain.ValueObjects;
using Xunit;

namespace Turnwise.Domain.Tests;

public class DiceFormulaTests
{
    [Theory]
    [InlineData("1d20+4", 1, 20, 4)]
    [InlineData("2d6-1", 2, 6, -1)]
    [InlineData("1d20", 1, 20, 0)]
    [InlineData("10d10+10", 10, 10, 10)]
    public void TryParse_ParsesValidNotation(string notation, int expectedCount, int expectedSize, int expectedModifier)
    {
        var parsed = DiceFormula.TryParse(notation, out var formula);

        Assert.True(parsed);
        Assert.Equal(expectedCount, formula!.DiceCount);
        Assert.Equal(expectedSize, formula.DieSize);
        Assert.Equal(expectedModifier, formula.Modifier);
    }

    [Theory]
    [InlineData("")]
    [InlineData("d20")]
    [InlineData("1d")]
    [InlineData("advantage")]
    [InlineData("1d20++4")]
    public void TryParse_RejectsInvalidNotation(string? notation)
    {
        var parsed = DiceFormula.TryParse(notation, out var formula);

        Assert.False(parsed);
        Assert.Null(formula);
    }

    [Fact]
    public void ToString_RoundTripsNotation()
    {
        var formula = DiceFormula.Parse("1d20+4");

        Assert.Equal("1d20+4", formula.ToString());
    }
}
