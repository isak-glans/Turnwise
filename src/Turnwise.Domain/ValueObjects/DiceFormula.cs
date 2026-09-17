using System.Text.RegularExpressions;

namespace Turnwise.Domain.ValueObjects;

/// <summary>
/// Simple dice notation in the form "NdX+Y" or "NdX-Y" (e.g. "1d20+4").
/// Advantage/disadvantage and other complex mechanics are explicitly out of scope for v1.
/// </summary>
public sealed partial record DiceFormula
{
    public int DiceCount { get; }
    public int DieSize { get; }
    public int Modifier { get; }

    private DiceFormula(int diceCount, int dieSize, int modifier)
    {
        DiceCount = diceCount;
        DieSize = dieSize;
        Modifier = modifier;
    }

    /// <summary>Notation longer than this can't describe a valid formula (bounds are far smaller) - rejected up front so we never even try to parse it. Also usable as an input's HTML maxlength.</summary>
    public const int MaxNotationLength = 20;

    private const int MinModifier = -999;
    private const int MaxModifier = 999;

    /// <summary>
    /// Never throws, regardless of input - including on digit runs too large for <see cref="int"/>
    /// (e.g. a pasted "1d6+99999999999999"), which would otherwise surface as an unhandled
    /// <see cref="OverflowException"/> to every caller that relies on the "Try" contract.
    /// </summary>
    public static bool TryParse(string? notation, out DiceFormula? formula)
    {
        formula = null;
        if (string.IsNullOrWhiteSpace(notation) || notation.Trim().Length > MaxNotationLength)
        {
            return false;
        }

        var match = NotationRegex().Match(notation.Trim());
        if (!match.Success)
        {
            return false;
        }

        if (!int.TryParse(match.Groups["count"].Value, out var diceCount) ||
            !int.TryParse(match.Groups["size"].Value, out var dieSize))
        {
            return false;
        }

        var modifier = 0;
        if (match.Groups["modSign"].Success && match.Groups["modValue"].Success)
        {
            if (!int.TryParse(match.Groups["modValue"].Value, out modifier))
            {
                return false;
            }

            if (match.Groups["modSign"].Value == "-")
            {
                modifier = -modifier;
            }
        }

        if (diceCount is < 1 or > 100 || dieSize is < 2 or > 1000 || modifier is < MinModifier or > MaxModifier)
        {
            return false;
        }

        formula = new DiceFormula(diceCount, dieSize, modifier);
        return true;
    }

    public static DiceFormula Parse(string notation)
    {
        if (!TryParse(notation, out var formula))
        {
            throw new FormatException($"'{notation}' is not a valid dice formula (expected NdX+Y, e.g. 1d20+4).");
        }

        return formula!;
    }

    public override string ToString()
    {
        var sign = Modifier switch
        {
            > 0 => "+",
            < 0 => "-",
            _ => ""
        };
        var modifierPart = Modifier == 0 ? "" : $"{sign}{Math.Abs(Modifier)}";
        return $"{DiceCount}d{DieSize}{modifierPart}";
    }

    [GeneratedRegex(@"^(?<count>\d+)d(?<size>\d+)(?:(?<modSign>[+-])(?<modValue>\d+))?$", RegexOptions.IgnoreCase)]
    private static partial Regex NotationRegex();
}
