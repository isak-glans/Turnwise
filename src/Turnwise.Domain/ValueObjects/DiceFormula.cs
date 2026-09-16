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

    public static bool TryParse(string? notation, out DiceFormula? formula)
    {
        formula = null;
        if (string.IsNullOrWhiteSpace(notation))
        {
            return false;
        }

        var match = NotationRegex().Match(notation.Trim());
        if (!match.Success)
        {
            return false;
        }

        var diceCount = int.Parse(match.Groups["count"].Value);
        var dieSize = int.Parse(match.Groups["size"].Value);
        var modifier = 0;
        if (match.Groups["modSign"].Success && match.Groups["modValue"].Success)
        {
            modifier = int.Parse(match.Groups["modValue"].Value);
            if (match.Groups["modSign"].Value == "-")
            {
                modifier = -modifier;
            }
        }

        if (diceCount is < 1 or > 100 || dieSize is < 2 or > 1000)
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
