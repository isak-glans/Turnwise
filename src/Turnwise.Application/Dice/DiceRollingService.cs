using Turnwise.Application.Abstractions;
using Turnwise.Domain.ValueObjects;

namespace Turnwise.Application.Dice;

/// <summary>Rolls dice formulas using an injected randomness source (kept swappable for deterministic tests).</summary>
public sealed class DiceRollingService(IRandomSource random)
{
    public DiceRollResult Roll(DiceFormula formula)
    {
        var rolls = new int[formula.DiceCount];
        for (var i = 0; i < formula.DiceCount; i++)
        {
            rolls[i] = random.Next(1, formula.DieSize);
        }

        var total = rolls.Sum() + formula.Modifier;
        return new DiceRollResult(rolls, formula.Modifier, total, formula.ToString());
    }
}
