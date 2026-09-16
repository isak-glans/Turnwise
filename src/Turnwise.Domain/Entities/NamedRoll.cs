using Turnwise.Domain.ValueObjects;

namespace Turnwise.Domain.Entities;

/// <summary>A reusable, named dice roll belonging to a combatant (e.g. "Longsword hit" -> 1d20+4).</summary>
public sealed class NamedRoll
{
    public Guid Id { get; }
    public string Name { get; set; }
    public DiceFormula Formula { get; set; }

    public NamedRoll(string name, DiceFormula formula, Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Roll name cannot be empty.", nameof(name));
        }

        Id = id ?? Guid.NewGuid();
        Name = name;
        Formula = formula;
    }
}
