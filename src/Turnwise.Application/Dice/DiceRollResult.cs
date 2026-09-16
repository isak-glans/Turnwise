namespace Turnwise.Application.Dice;

/// <summary>The outcome of rolling a dice formula: each individual die, the modifier, and the total.</summary>
public sealed record DiceRollResult(IReadOnlyList<int> IndividualRolls, int Modifier, int Total, string FormulaText)
{
    public override string ToString() =>
        $"{FormulaText}: [{string.Join(", ", IndividualRolls)}]{(Modifier != 0 ? $" {(Modifier > 0 ? "+" : "-")} {Math.Abs(Modifier)}" : "")} = {Total}";
}
