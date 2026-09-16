using Turnwise.Application.Dice;
using Turnwise.Domain.ValueObjects;
using Xunit;

namespace Turnwise.Application.Tests;

public class DiceRollingServiceTests
{
    [Fact]
    public void Roll_SumsIndividualDiceAndAddsModifier()
    {
        var random = new FakeRandomSource(4, 6, 1); // three d-somethings
        var service = new DiceRollingService(random);
        var formula = DiceFormula.Parse("3d6+2");

        var result = service.Roll(formula);

        Assert.Equal([4, 6, 1], result.IndividualRolls);
        Assert.Equal(2, result.Modifier);
        Assert.Equal(13, result.Total); // 4+6+1+2
    }

    [Fact]
    public void Roll_NegativeModifierReducesTotal()
    {
        var random = new FakeRandomSource(10);
        var service = new DiceRollingService(random);
        var formula = DiceFormula.Parse("1d20-3");

        var result = service.Roll(formula);

        Assert.Equal(7, result.Total);
    }
}
