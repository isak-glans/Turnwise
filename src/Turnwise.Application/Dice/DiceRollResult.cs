namespace Turnwise.Application.Dice;

/// <summary>The outcome of rolling a dice formula: each individual die, the modifier, and the total.</summary>
public sealed record DiceRollResult(IReadOnlyList<int> IndividualRolls, int Modifier, int Total, string FormulaText)
{
    /// <summary>The dice + modifier breakdown without the total (e.g. "[14] + 3") - for messages that show the total separately.</summary>
    public string Breakdown =>
        $"[{string.Join(", ", IndividualRolls)}]{(Modifier != 0 ? $" {(Modifier > 0 ? "+" : "-")} {Math.Abs(Modifier)}" : "")}";

    public override string ToString() => $"{FormulaText}: {Breakdown} = {Total}";
}
