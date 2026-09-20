using System.Diagnostics.CodeAnalysis;
using Turnwise.Domain.Enums;
using Turnwise.Domain.ValueObjects;

namespace Turnwise.Domain.Entities;

/// <summary>A reusable, named dice roll belonging to a combatant (e.g. "Longsword hit" -> 1d20+4).</summary>
public sealed class NamedRoll
{
    public const int MaxNameLength = 100;

    private string _name;

    public Guid Id { get; }

    /// <summary>Silently truncated to <see cref="MaxNameLength"/> rather than rejected, since this can be set on every keystroke while editing.</summary>
    public string Name
    {
        get => _name;
        [MemberNotNull(nameof(_name))]
        set => _name = Truncate(value);
    }

    public DiceFormula Formula { get; set; }

    /// <summary>Optional Weapon/Magic/Skill tag, used to pick an icon for this roll's row and its combat log entries.</summary>
    public NamedRollCategory? Category { get; set; }

    public NamedRoll(string name, DiceFormula formula, Guid? id = null, NamedRollCategory? category = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Roll name cannot be empty.", nameof(name));
        }

        Id = id ?? Guid.NewGuid();
        Name = name;
        Formula = formula;
        Category = category;
    }

    private static string Truncate(string? value)
    {
        var text = value ?? "";
        return text.Length > MaxNameLength ? text[..MaxNameLength] : text;
    }
}
